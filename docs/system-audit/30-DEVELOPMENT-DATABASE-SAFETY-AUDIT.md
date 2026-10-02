# Development Database Safety Audit

## Scope and safety

Audit-only. Không chạy API, không kết nối PostgreSQL/Supabase, không chạy migration/seed, không sửa source/config/User Secrets/environment, không tạo database và không commit/push.

## 1. Database configuration

services/api/Program.cs đăng ký ApplicationDbContext bằng Npgsql:

    GetConnectionString("DefaultConnection")
    -> fallback Configuration["SUPABASE_CONNECTION_STRING"]
    -> normalize PostgreSQL URI/keyword format
    -> AddDbContext<ApplicationDbContext>(UseNpgsql)

Configuration precedence thực tế của WebApplication.CreateBuilder(args):

    appsettings.json
    -> appsettings.{Environment}.json
    -> User Secrets (Development)
    -> environment variables
    -> command-line arguments

Evidence:

- services/api/appsettings.json có ConnectionStrings:DefaultConnection rỗng, không chứa credential.
- Không có appsettings.Development.json trong API project.
- RestaurantPOS.api.csproj có UserSecretsId.
- Source hỗ trợ ConnectionStrings:DefaultConnection, SUPABASE_CONNECTION_STRING, Jwt:Secret, Seed:AdminPassword và các key HTTPS.
- Không có localhost database fallback trong source.

## 2. Current configuration source

| Source | Status | Evidence |
|---|---|---|
| Tracked config | PRESENT, empty placeholder | services/api/appsettings.json |
| User Secrets | PRESENT | Secure configuration keys tồn tại; values không ghi vào report |
| Environment variable | SUPPORTED | ConnectionStrings__DefaultConnection, SUPABASE_CONNECTION_STRING, seed fallback |
| Unknown external override | POSSIBLE | ASP.NET command-line/config providers |

Provider theo source: PostgreSQL qua Npgsql, với contract/configuration ghi rõ Supabase PostgreSQL. Không network check nên không xác nhận reachability hoặc database instance hiện tại.

## 3. Startup database behavior

Startup order:

    CreateBuilder
    -> validate Jwt:Secret
    -> resolve/validate PostgreSQL connection
    -> register DbContext
    -> validate HTTPS certificate
    -> builder.Build()
    -> create scope
    -> db.Database.Migrate()
    -> await DbInitializer.SeedAsync(...)
    -> failure throws and API does not serve
    -> configure middleware/endpoints/SignalR
    -> app.Run()

EnsureCreated, EnsureDeleted và InitializeDatabase không được dùng trong API startup. EnsureCreated chỉ xuất hiện trong tests/RestaurantPOS.Tests/TestDbContextFactory.cs với SQLite in-memory.

## 4. Migration safety

- db.Database.Migrate() được gọi unconditional, không có IsDevelopment gate.
- Behavior này áp dụng cho mọi environment khi API startup.
- Migration/seed exception được wrap thành InvalidOperationException, API không tiếp tục serve.
- Nếu connection trỏ remote Supabase, startup có thể mutate database bằng migration pending.

Migration history có destructive operations (DropTable, DropColumn, raw SQL), gồm:

- 20260919075618_RemovePayrollAndRepairReceiptSettings trong migration tree bổ sung, có drop Payroll-related tables/columns;
- các migration corrective/repair cũ có drop/recreate operations;
- lịch sử vẫn chứa Payroll migrations như 20260906143000_AddPayroll, 20260907060600_AddMissingPayrollsAndSettings và 20260907155909_RepairMissingDatabaseSchema.

Không chạy migration nên không kết luận database hiện tại đã apply migration nào.

### Split migration trees — HUMAN REVIEW

Có hai cây migration source:

- services/api/src/Infrastructure/Persistence/Migrations, namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;
- services/api/Infrastructure/Persistence/Migrations, namespace RestaurantPOS.api.Infrastructure.Persistence.Migrations.

ApplicationDbContext nằm trong RestaurantPOS.Infrastructure.Persistence, còn SDK project mặc định compile .cs trong project tree. Đây là migration discovery/history risk; không tự chọn, xóa hoặc hợp nhất cây migration trong audit.

## 5. Seed safety

Seeder: services/api/src/Infrastructure/Persistence/DbInitializer.cs.

- SeedAsync được gọi sau Migrate ở mọi environment.
- Main seed block chạy khi !Branches.AnyAsync().
- Tạo branch chính, areas, tables trạng thái trống và products cơ bản.
- Nếu chưa có employee, default admin chỉ được tạo ở Development.
- Admin password lấy từ Seed:AdminPassword, fallback RESTAURANTPOS_SEED_ADMIN_PASSWORD.
- Thiếu seed password khi cần tạo admin sẽ throw.
- Không thấy Customer, Order, Payment hoặc Kitchen request seed trong DbInitializer.
- Seeder không overwrite rows khi branch đã tồn tại, nhưng không idempotent theo từng entity: nếu branch tồn tại mà product/table thiếu thì outer gate không tự bù.
- Không có default password literal trong source.

## 6. Minimum development dataset for E2E

| Data | Need | Reason |
|---|---|---|
| Branch | Required | branch isolation/order context |
| RestaurantTable | Required for QR | tableId resolution/status |
| Area | Recommended | POS/table context |
| Active Product + category | Required | menu/category/order |
| Size/topping data | Optional, recommended | options payload coverage |
| Admin/manager/cashier employee | Required | staff/POS login |
| Kitchen employee | Required | KDS/SignalR auth |
| Customer | Optional | registered flow; guest does not need pre-seeded customer |
| Order/OrderRequest | Test-created only | customer -> POS -> Kitchen |
| Payment data | Payment test only | cash/bank-transfer source flow |

Không tạo dữ liệu trong audit này.

## 7. Safe environment options

### A. Current Supabase database

Setup effort thấp, compatibility cao, nhưng risk cao/unknown vì có unrestricted startup migration/seed và chưa xác nhận disposable. Không phù hợp E2E hiện tại.

### B. Separate Supabase development/test project

Setup effort moderate, risk thấp nếu project/user/database riêng, compatibility cao nhất với Npgsql/Supabase hiện tại, migration/seed hỗ trợ đầy đủ. Phù hợp nhất cho E2E.

### C. Local PostgreSQL

Setup effort moderate/high vì Docker ngoài scope và phải cài PostgreSQL thủ công; risk thấp nếu database/user riêng; Npgsql compatibility cao nhưng khác một số operational behavior của Supabase. Fallback tốt.

## 8. Recommended approach

Khuyến nghị B — separate Supabase development/test project:

1. Giống provider/runtime hiện tại.
2. Không ảnh hưởng dữ liệu hiện có.
3. Cho phép migration/seed và test order/payment trên database disposable.
4. Không cần Docker.

Local PostgreSQL là phương án thay thế nếu không thể tạo Supabase project riêng; source không tự fallback localhost nên phải cấu hình rõ ràng.

## 9. User Secrets templates

Các command sau chỉ là template, không chạy trong audit:

    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<SAFE_DEVELOPMENT_POSTGRES_CONNECTION_STRING>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:Secret" "<RANDOM_DEVELOPMENT_JWT_SECRET>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:Issuer" "RestaurantPOS" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:Audience" "RestaurantPOS" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:ExpiryDays" "7" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Seed:AdminPassword" "<DEVELOPMENT_ONLY_ADMIN_PASSWORD>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Gemini:ApiKey" "<OPTIONAL_GEMINI_KEY>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "HTTPS_CERT_PATH" "<OPTIONAL_CERT_PATH>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "HTTPS_CERT_PASSWORD" "<OPTIONAL_CERT_PASSWORD>" --project .\services\api\RestaurantPOS.api.csproj

Fallback keys thực tế: SUPABASE_CONNECTION_STRING và RESTAURANTPOS_SEED_ADMIN_PASSWORD. Gemini không required cho QR/POS/Kitchen/Payment flow.

## 10. Startup hardening recommendations

### REQUIRED BEFORE E2E

1. Confirm/create isolated development database.
2. Resolve/review split migration trees and intended migration assembly.
3. Review destructive pending migrations against isolated database only.
4. Configure development JWT, connection and seed password securely.

### RECOMMENDED HARDENING

1. Gate automatic migration behind explicit Development-only opt-in hoặc chuyển sang controlled migration command.
2. Không auto-migrate production/shared startup; migration phải được review/deploy riêng.
3. Make seed explicitly Development-only and split seed gates per entity group.
4. Add guard rejecting known production/shared database markers in E2E config.

### OPTIONAL

1. Disposable database reset workflow.
2. PostgreSQL integration profile against isolated DB.
3. Browser E2E tooling after database/migration safety is resolved.

## 11. E2E readiness

Human actions required:

1. Create/select separate Supabase development/test project or dedicated local PostgreSQL.
2. Configure safe ConnectionStrings:DefaultConnection.
3. Configure Development JWT and seed admin password.
4. Confirm HTTPS certificate and frontend API URLs.
5. Resolve/review migration trees and approve isolated migration.
6. Run migration/seed only on confirmed safe DB.
7. Verify Branch/Table/Product/Employee fixtures, then run customer/POS/Kitchen flow.
8. Review and clean test records only after relationship/side-effect review.

## Final summary

    DATABASE PROVIDER:
    PostgreSQL via Npgsql; Supabase PostgreSQL configuration contract

    CONNECTION CONFIG KEY:
    ConnectionStrings:DefaultConnection

    CURRENT CONFIG SOURCE:
    USER SECRET (tracked config is empty placeholder; environment fallback supported)

    REMOTE DATABASE:
    YES / UNKNOWN SAFETY — Supabase contract; no network check

    AUTO MIGRATION:
    YES

    AUTO SEED:
    YES

    AUTO MIGRATION ENVIRONMENT:
    ALL ENVIRONMENTS

    AUTO SEED ENVIRONMENT:
    ALL ENVIRONMENTS invoke SeedAsync; default admin creation is Development-only

    SEED IDEMPOTENT:
    PARTIAL

    SEED ADMIN PASSWORD SOURCE:
    Seed:AdminPassword, fallback RESTAURANTPOS_SEED_ADMIN_PASSWORD

    SAFE FOR CURRENT E2E:
    NO

    RECOMMENDED E2E DATABASE:
    Separate Supabase development/test project; dedicated local PostgreSQL fallback

    REQUIRED HUMAN ACTIONS:
    1. Confirm/create isolated development database.
    2. Review split migration trees and approve migration target.
    3. Configure safe User Secrets and seed fixtures.

    RECOMMENDED STARTUP HARDENING:
    1. Remove unrestricted automatic migration from non-development startup.
    2. Make seed explicitly Development-only and independently idempotent.
    3. Resolve duplicate/split migration assembly before runtime migration.

    RUNTIME E2E STATUS:
    BLOCKED

