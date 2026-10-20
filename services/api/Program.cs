using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Infrastructure.Services;
using RestaurantPOS.Infrastructure.Configuration;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Diagnostics;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Tools.Admin;
using RestaurantPOS.AI.Tools.Customer;
using RestaurantPOS.AI.Security;
using RestaurantPOS.WebAPI.Hubs;
using RestaurantPOS.WebAPI.Middleware;
using RestaurantPOS.WebAPI.Configuration;
using RestaurantPOS.WebAPI.Diagnostics;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using RestaurantPOS.Domain.Entities;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("BACKEND_STARTUP_PHASE=CONFIGURATION_LOADING");
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() &&
    builder.Configuration.GetValue<bool>("StartupDiagnostics:ValidateDependencyInjection"))
{
    builder.Host.UseDefaultServiceProvider((_, options) =>
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    });
    Console.WriteLine("BACKEND_STARTUP_DI_VALIDATION=ENABLED");
}

// Development-only, allow-listed AI diagnostics. The provider writes selected
// fields only; it is never registered in Production and never captures raw logs.
var uatLoggingEnabled = builder.Configuration.GetValue<bool>("AI:UatLogging:Enabled");
var safeAiDiagnosticPath = builder.Configuration["AI:UatLogging:Path"] ?? "logs/ai-uat/ai-chat.log";
var safeAiDiagnosticMaxBytes = builder.Configuration.GetValue<long?>("AI:UatLogging:MaxFileBytes") ?? 5 * 1024 * 1024;
var safeAiDiagnosticRetainedFiles = builder.Configuration.GetValue<int?>("AI:UatLogging:RetainedFiles") ?? 3;
if ((builder.Environment.IsDevelopment() || string.Equals(builder.Environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) || string.Equals(builder.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase)) && uatLoggingEnabled)
{
    var resolvedPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, safeAiDiagnosticPath));
    builder.Logging.AddProvider(new SafeAiDiagnosticFileLoggerProvider(resolvedPath, safeAiDiagnosticMaxBytes, safeAiDiagnosticRetainedFiles));
}

Console.WriteLine("BACKEND_STARTUP_PHASE=SERVICE_REGISTRATION");
// Add services to the container.
builder.Services.AddControllers();
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
        },
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            var role = principal?.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
            var userIdValue = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!RoleNames.IsKnownRole(role))
            {
                context.Fail("The role claim is not recognized.");
                return;
            }

            if (RoleNames.IsStaffRole(role))
            {
                if (!Guid.TryParse(userIdValue, out var employeeId))
                {
                    context.Fail("The employee identity claim is invalid.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var employee = await db.Employees.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == employeeId, context.HttpContext.RequestAborted);
                if (employee == null)
                {
                    context.Fail("The employee account is inactive or no longer exists.");
                    return;
                }

                if (!RoleNames.IsValidStaffTokenState(
                        role,
                        employee.Role,
                        employee.IsActive,
                        employee.BranchId,
                        principal?.FindFirst("branchId")?.Value))
                {
                    context.Fail("The employee authentication state does not match the current server state.");
                }
                return;
            }

            if (string.Equals(role, RoleNames.Customer, StringComparison.OrdinalIgnoreCase))
            {
                var sessionType = principal?.FindFirst("customerSessionType")?.Value;
                if (string.Equals(sessionType, "guest", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(sessionType, "directGuest", StringComparison.OrdinalIgnoreCase))
                    return;

                if (!string.Equals(sessionType, "registered", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(principal?.FindFirst("customerAuthMethod")?.Value, "password", StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail("The registered customer session is not password-authenticated.");
                    return;
                }

                if (!Guid.TryParse(userIdValue, out var customerId))
                {
                    context.Fail("The customer identity claim is invalid.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var customer = await db.Customers.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == customerId, context.HttpContext.RequestAborted);
                if (customer != null && !customer.IsActive)
                    context.Fail("The customer account is inactive.");
                else if (customer == null && string.Equals(sessionType, "registered", StringComparison.OrdinalIgnoreCase))
                    context.Fail("The customer account no longer exists.");
            }
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
builder.Services.AddScoped<IToppingService, ToppingService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IKitchenService, KitchenService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDashboardCacheInvalidator>(sp => sp.GetRequiredService<IDashboardService>());
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IKitchenNotifier, KitchenNotifier>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Đăng ký Password Hasher
builder.Services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();
builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();

// 2. Đăng ký các AI Core Services
builder.Services.AddScoped<AiCorrelationContext>();
builder.Services.AddScoped<AiPermissionService>();
builder.Services.AddSingleton<AdminEmbeddingPilot>();
builder.Services.AddSingleton<CustomerGuestEmbeddingPilot>();
builder.Services.AddSingleton<IAiToolExecutionAuditSink, AiToolExecutionAuditSink>();
builder.Services.AddScoped<IAiOrchestrator>(serviceProvider =>
    new AiOrchestrator(
        serviceProvider.GetRequiredService<AiToolRegistry>(),
        serviceProvider.GetRequiredService<AiPermissionService>(),
        serviceProvider.GetRequiredService<AdminEmbeddingPilot>(),
        serviceProvider.GetRequiredService<CustomerGuestEmbeddingPilot>(),
        serviceProvider.GetRequiredService<ILogger<AiOrchestrator>>(),
        serviceProvider.GetRequiredService<IAiToolExecutionAuditSink>(),
        serviceProvider.GetRequiredService<AiCorrelationContext>()));
builder.Services.AddScoped<AiAuthorization>();
builder.Services.AddSingleton<IProductPriceConfirmationStore, ProductPriceConfirmationStore>();
builder.Services.AddSingleton<IProductNameConfirmationStore, ProductNameConfirmationStore>();
builder.Services.AddSingleton<ICatalogCreationConfirmationStore, CatalogCreationConfirmationStore>();

// 3. Đăng ký AI Tool Registry
builder.Services.AddScoped<AiToolRegistry>();

// 4. Đăng ký TẤT CẢ các AI Tools
// Admin Tools
builder.Services.AddScoped<IAiTool, GetRevenueTool>();
builder.Services.AddScoped<IAiTool, GetOrderListTool>();
builder.Services.AddScoped<IAiTool, GetBestSellersTool>();
builder.Services.AddScoped<IAiTool, GetRevenueComparisonTool>();
builder.Services.AddScoped<IAiTool, GetBusinessSummaryTool>();
builder.Services.AddScoped<IAiTool, UpdateProductPriceTool>();
builder.Services.AddScoped<IAiTool, UpdateProductNameTool>();
builder.Services.AddScoped<IAiTool, CreateProductTool>();
builder.Services.AddScoped<IAiTool, CreateToppingTool>();

// Employee Tools

// Customer Tools
builder.Services.AddScoped<IAiTool, GetMenuTool>();
builder.Services.AddScoped<IAiTool, GetMyOrderTool>();

Console.WriteLine("BACKEND_STARTUP_PHASE=PERSISTENCE_CONFIGURATION");
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
var parsedConnectionString = new NpgsqlConnectionStringBuilder(connectionString);
SupabaseProjectTargetGuard.Validate(parsedConnectionString, builder.Configuration, builder.Environment.IsDevelopment());
connectionString = PostgresConnectionPoolPolicy.Apply(connectionString, builder.Configuration);

// Capture the effective TEST target from this process, then verify it with a
// read-only connection before registering the same connection for EF Core.
if (builder.Environment.IsDevelopment() &&
    string.Equals(builder.Configuration["Database:TargetMode"], SupabaseProjectTargetGuard.TestMode, StringComparison.OrdinalIgnoreCase))
{
    var targetAttestation = await DatabaseTargetAttestationWriter.WriteAsync(
        builder.Environment,
        builder.Configuration,
        new NpgsqlConnectionStringBuilder(connectionString));
    Console.WriteLine($"BACKEND_DATABASE_TARGET_ATTESTATION={targetAttestation.Status} PATH={targetAttestation.Path}");
}

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
var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>()).ToList();
if (builder.Environment.IsDevelopment())
{
    var configuredLanHost = builder.Configuration["DOAN_DEV_LAN_HOST"]?.Trim();
    if (!string.IsNullOrWhiteSpace(configuredLanHost) &&
        System.Text.RegularExpressions.Regex.IsMatch(configuredLanHost, "^[A-Za-z0-9.-]+$"))
    {
        allowedOrigins.Add($"https://{configuredLanHost}:5173");
        allowedOrigins.Add($"https://{configuredLanHost}:5174");
    }
}
allowedOrigins = allowedOrigins.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins(allowedOrigins.ToArray())
                      .SetIsOriginAllowed(origin =>
                      {
                          // Development LAN origins must be explicitly listed in configuration.
                          // This keeps the policy restrictive while supporting the configured Vite hosts.
                          return !string.IsNullOrWhiteSpace(origin)
                              && allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                      });
            }
            else
            {
                policy.WithOrigins(allowedOrigins.ToArray());
            }

            policy.AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("X-Has-Next-Page")
                  .AllowCredentials(); // Bắt buộc cho SignalR
        });
});

Console.WriteLine("BACKEND_STARTUP_PHASE=HTTPS_CONFIGURATION");
var configuredPemCertificatePath = builder.Configuration["HTTPS_PEM_CERT_PATH"];
var configuredPemKeyPath = builder.Configuration["HTTPS_PEM_KEY_PATH"];
Console.WriteLine($"HTTPS environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"ASPNETCORE_URLS configured: {!string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_URLS"])}");
if (string.IsNullOrWhiteSpace(configuredPemCertificatePath) || string.IsNullOrWhiteSpace(configuredPemKeyPath))
{
    Console.WriteLine("BACKEND_HTTPS_CERTIFICATE_VALID=NO");
    throw new InvalidOperationException("HTTPS PEM paths are missing. Run .\\scripts\\setup-https.ps1 in this PowerShell session before starting the API.");
}

Console.WriteLine($"HTTPS certificate path: {configuredPemCertificatePath}");
var pemCertificateExists = File.Exists(configuredPemCertificatePath);
var pemKeyExists = File.Exists(configuredPemKeyPath);
Console.WriteLine($"HTTPS certificate exists: {pemCertificateExists}");
if (!pemCertificateExists || !pemKeyExists)
{
    Console.WriteLine("BACKEND_HTTPS_CERTIFICATE_VALID=NO");
    throw new InvalidOperationException("HTTPS PEM certificate or private key is missing.");
}

System.Security.Cryptography.X509Certificates.X509Certificate2 httpsCertificate;
try
{
    httpsCertificate = DevelopmentHttpsCertificateLoader.LoadPersistedPkcs12FromPem(
        configuredPemCertificatePath,
        configuredPemKeyPath);
}
catch (Exception exception)
{
    Console.WriteLine($"BACKEND_STARTUP_FAILURE_PHASE=HTTPS_CONFIGURATION BACKEND_EXCEPTION_TYPE={GetSafeStartupExceptionType(exception)} BACKEND_HTTPS_CERTIFICATE_VALID=NO");
    throw;
}

if (!httpsCertificate.HasPrivateKey)
    throw new InvalidOperationException("The Development PEM certificate does not contain a private key.");

Console.WriteLine("BACKEND_HTTPS_CERTIFICATE_VALID=YES");

builder.WebHost.ConfigureKestrel(options =>
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.UseHttps(httpsCertificate);
    }));

Console.WriteLine("BACKEND_STARTUP_PHASE=DI_BUILD");
WebApplication app;
try
{
    app = builder.Build();
}
catch (Exception exception)
{
    WriteSafeDependencyFailureDiagnostics(exception);
    Console.WriteLine($"BACKEND_STARTUP_FAILURE_PHASE=DI_BUILD BACKEND_EXCEPTION_TYPE={GetSafeStartupExceptionType(exception)}");
    throw;
}

// Database schema/data mutation is an explicit Development-only opt-in.
// This prevents a normal startup, especially a production/main-DB startup,
// from applying migrations or seed data implicitly.
var allowAutomaticDatabaseMutation = builder.Configuration.GetValue<bool>("DatabaseSafety:AllowAutomaticMigration");
if (app.Environment.IsDevelopment() && allowAutomaticDatabaseMutation)
{
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
                "Database migration/seeding failed. Verify the isolated Development database configuration.", ex);
        }
    }
}
else
{
    Console.WriteLine("Automatic database migration/seeding is disabled. Set DatabaseSafety:AllowAutomaticMigration=true only for an isolated Development database.");
}

// Hiển thị lỗi chi tiết khi chạy Local để dễ Debug
// Keep intermittent Auth/me failures observable without exposing claims or
// changing the response/authorization pipeline.
app.UseMiddleware<SafeAuthMeDiagnosticsMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant POS API V1");
        c.RoutePrefix = "swagger";
    });
}

// Keep GET /api/Order failures observable without exposing exception details.
app.UseMiddleware<SafeOrderRequestDiagnosticsMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// app.UseHttpsRedirection(); // Tắt cái này khi chạy Local để tránh lỗi Certificate phức tạp
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<KitchenHub>("/kitchenHub");

// Kestrel owns the only API endpoint: HTTPS on port 5000.
Console.WriteLine("BACKEND_STARTUP_PHASE=APPLICATION_STARTUP");
try
{
    app.Run();
}
catch (Exception exception)
{
    Console.WriteLine($"BACKEND_STARTUP_FAILURE_PHASE=APPLICATION_STARTUP BACKEND_EXCEPTION_TYPE={GetSafeStartupExceptionType(exception)}");
    throw;
}

static string GetSafeStartupExceptionType(Exception exception)
{
    for (var current = exception; current is not null; current = current.InnerException)
    {
        var typeName = current.GetType().Name;
        if (typeName is "AuthenticationException"
            or "CryptographicException"
            or "FileNotFoundException"
            or "InvalidOperationException"
            or "IOException"
            or "SocketException"
            or "TypeInitializationException")
        {
            return typeName;
        }
    }

    return "UnknownException";
}

static void WriteSafeDependencyFailureDiagnostics(Exception exception)
{
    foreach (var current in EnumerateExceptionChain(exception))
    {
        var message = current.Message ?? string.Empty;
        var serviceType = MatchSafeTypeName(
            message,
            @"ServiceType:\s*(?<value>[^,]+),\s*Lifetime:");
        var implementationType = MatchSafeTypeName(
            message,
            @"ImplementationType:\s*(?<value>[^']+)");
        var dependencyType = MatchSafeTypeName(
            message,
            @"(?:Unable to resolve service for type|Cannot consume scoped service)\s*'(?<value>[^']+)'");
        var ownerType = MatchSafeTypeName(
            message,
            @"(?:while attempting to activate|from singleton)\s*'(?<value>[^']+)'");

        if (serviceType is not null || implementationType is not null || dependencyType is not null)
        {
            var category = dependencyType is not null && ownerType is not null
                ? "INVALID_LIFETIME_OR_DEPENDENCY"
                : "SERVICE_CONSTRUCTION_FAILURE";

            Console.WriteLine(
                $"BACKEND_DI_FAILURE_CATEGORY={category} " +
                $"BACKEND_DI_SERVICE_TYPE={serviceType ?? "UNKNOWN"} " +
                $"BACKEND_DI_IMPLEMENTATION_TYPE={implementationType ?? "UNKNOWN"} " +
                $"BACKEND_DI_DEPENDENCY_TYPE={dependencyType ?? "UNKNOWN"} " +
                $"BACKEND_DI_OWNER_TYPE={ownerType ?? "UNKNOWN"}");
            return;
        }
    }

    Console.WriteLine("BACKEND_DI_FAILURE_CATEGORY=UNCLASSIFIED BACKEND_DI_SERVICE_TYPE=UNKNOWN BACKEND_DI_IMPLEMENTATION_TYPE=UNKNOWN BACKEND_DI_DEPENDENCY_TYPE=UNKNOWN BACKEND_DI_OWNER_TYPE=UNKNOWN");
}

static IEnumerable<Exception> EnumerateExceptionChain(Exception exception)
{
    for (var current = exception; current is not null; current = current.InnerException)
        yield return current;
}

static string? MatchSafeTypeName(string message, string pattern)
{
    var match = System.Text.RegularExpressions.Regex.Match(
        message,
        pattern,
        System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    if (!match.Success)
        return null;

    var value = match.Groups["value"].Value.Trim();
    if (value.Length == 0 || value.Length > 256)
        return null;

    var safeValue = new string(value
        .Where(character => char.IsLetterOrDigit(character) || "._,+<>[]` ".Contains(character))
        .ToArray())
        .Trim();

    return safeValue.Length == 0 ? null : safeValue;
}

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
