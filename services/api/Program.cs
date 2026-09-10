using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Infrastructure.Services;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Tools.Admin;
using RestaurantPOS.AI.Tools.Employee;
using RestaurantPOS.AI.Tools.Customer;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Prompts;
using RestaurantPOS.WebAPI.Hubs;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using RestaurantPOS.Domain.Entities;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<RestaurantPOS.Application.Services.IPayrollService, RestaurantPOS.Application.Services.PayrollService>();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

// Swagger với hỗ trợ JWT Bearer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Restaurant POS API",
        Version = "v1",
        Description = "API quản lý vận hành nhà hàng, bán hàng và khách hàng."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// Cấu hình JWT Bearer Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Jwt:Secret must be supplied through secure configuration (for example Jwt__Secret environment variable).");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RestaurantPOS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RestaurantPOS";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Local Development may run over HTTP; deployed environments must use HTTPS metadata.
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Hỗ trợ SignalR truyền token qua query string (?access_token=...)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/kitchenHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Rate Limiting policies
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // AI Limiter (Existing)
    options.AddPolicy("ai-limiter", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 15,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    // Strict Limiter (Login, Authentication)
    options.AddPolicy("strict-limiter", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    // Moderate Limiter (Customer lookup, registration)
    options.AddPolicy("moderate-limiter", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    // Order Limiter (Submission)
    options.AddPolicy("order-limiter", httpContext =>
    {
        // Partition by user identity if authenticated, otherwise by IP.
        var identity = httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(identity, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

// 0. Đăng ký Application Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IKitchenService, KitchenService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IKitchenNotifier, KitchenNotifier>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();

// Đăng ký Password Hasher
builder.Services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();
builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();

// 1. Cấu hình HttpClient cho Gemini Service
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// 2. Đăng ký các AI Core Services
builder.Services.AddScoped<AiContextService>();
builder.Services.AddScoped<AiPermissionService>();
builder.Services.AddScoped<IAiOrchestrator, AiOrchestrator>();
builder.Services.AddScoped<PromptBuilder>();
builder.Services.AddScoped<AiAuthorization>();

// 3. Đăng ký AI Tool Registry
builder.Services.AddScoped<AiToolRegistry>();

// 4. Đăng ký TẤT CẢ các AI Tools
// Admin Tools
builder.Services.AddScoped<IAiTool, GetRevenueTool>();
builder.Services.AddScoped<IAiTool, UpdateProductPriceTool>();
builder.Services.AddScoped<IAiTool, GetActiveStaffTool>();

// Employee Tools
builder.Services.AddScoped<IAiTool, GetTableSummaryTool>();
builder.Services.AddScoped<IAiTool, GetMyShiftTool>();
builder.Services.AddScoped<IAiTool, UpdateOrderStatusTool>();

// Customer Tools
builder.Services.AddScoped<IAiTool, GetMenuTool>();
builder.Services.AddScoped<IAiTool, GetMyOrderTool>();
builder.Services.AddScoped<IAiTool, CreateBookingTool>();

// Cấu hình kết nối PostgreSQL (Supabase). Never fall back to localhost or a
// bundled database; the connection string must come from secure configuration.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    connectionString = builder.Configuration["SUPABASE_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be supplied with the Supabase PostgreSQL connection string " +
        "(ConnectionStrings__DefaultConnection environment variable or .NET User Secrets)." );

// Supabase exposes both URI and keyword/value formats. Npgsql expects the
// latter here, so normalize a URI without exposing or persisting credentials.
connectionString = NormalizePostgresConnectionString(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(60); // Tăng thời gian chờ lệnh lên 60 giây
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

// Cấu hình CORS để Frontend có thể gọi API an toàn qua HTTPS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins(allowedOrigins)
                      .SetIsOriginAllowed(origin =>
                      {
                          if (string.IsNullOrWhiteSpace(origin)) return false;

                          // Hỗ trợ truy cập qua IP LAN động (HTTPS + Port 5173/5174)
                          if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                          {
                              return uri.Scheme == "https" && (uri.Port == 5173 || uri.Port == 5174);
                          }
                          return false;
                      });
            }
            else
            {
                policy.WithOrigins(allowedOrigins);
            }

            policy.AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Bắt buộc cho SignalR
        });
});

var configuredHttpsCertificatePath = builder.Configuration["HTTPS_CERT_PATH"];
var defaultHttpsCertificatePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".https", "pos-cert.pfx"));
var httpsCertificatePath = string.IsNullOrWhiteSpace(configuredHttpsCertificatePath)
    ? defaultHttpsCertificatePath
    : configuredHttpsCertificatePath;
var passwordFile = Path.Combine(Path.GetDirectoryName(httpsCertificatePath)!, "cert-password.txt");
var httpsCertificatePassword = File.Exists(passwordFile) ? File.ReadAllText(passwordFile).Trim() : (builder.Configuration["HTTPS_CERT_PASSWORD"] ?? "pos-dev-cert");
var httpsCertificateExists = File.Exists(httpsCertificatePath);
Console.WriteLine($"HTTPS environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"ASPNETCORE_URLS configured: {!string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_URLS"])}");
Console.WriteLine($"HTTPS certificate path: {httpsCertificatePath}");
Console.WriteLine($"HTTPS certificate exists: {httpsCertificateExists}");
if (!httpsCertificateExists)
    throw new InvalidOperationException("HTTPS certificate not found. Run .\\scripts\\setup-https.ps1 in this PowerShell session before starting the API.");

builder.WebHost.ConfigureKestrel(options =>
    options.ListenAnyIP(5000, listenOptions =>
        listenOptions.UseHttps(httpsCertificatePath, httpsCertificatePassword)));

var app = builder.Build();

// Tự động kiểm tra và cập nhật Database (Migration) khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
        await DbInitializer.SeedAsync(db, scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        // Do not start an API that cannot read/write its configured Supabase
        // database. The exception intentionally contains no connection data.
        throw new InvalidOperationException(
            "Database migration/seeding failed. Verify the Supabase PostgreSQL configuration.", ex);
    }
}

// Hiển thị lỗi chi tiết khi chạy Local để dễ Debug
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant POS API V1");
        c.RoutePrefix = "swagger";
    });
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// app.UseHttpsRedirection(); // Tắt cái này khi chạy Local để tránh lỗi Certificate phức tạp
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<KitchenHub>("/kitchenHub");

// Kestrel owns the only API endpoint: HTTPS on port 5000.
app.Run();

static string NormalizePostgresConnectionString(string rawConnectionString)
{
    var value = rawConnectionString.Trim();
    if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        value = value[1..^1];

    if (!value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        var keywordBuilder = new NpgsqlConnectionStringBuilder(value);
        // Supabase port 6543 is the transaction pooler. Avoid retaining a
        // client-side session across EF migration commands.
        if (keywordBuilder.Port == 6543)
            keywordBuilder.Pooling = false;
        return keywordBuilder.ConnectionString;
    }

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
        throw new InvalidOperationException("The Supabase PostgreSQL connection URI is invalid.");

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}
