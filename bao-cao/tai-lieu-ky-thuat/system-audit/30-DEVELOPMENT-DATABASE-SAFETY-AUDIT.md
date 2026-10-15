# Development Database Safety Audit

## Scope and safety

Audit-only. KhĂ´ng cháº¡y API, khĂ´ng káº¿t ná»‘i PostgreSQL/Supabase, khĂ´ng cháº¡y migration/seed, khĂ´ng sá»­a source/config/User Secrets/environment, khĂ´ng táº¡o database vĂ  khĂ´ng commit/push.

## 1. Database configuration

services/api/Program.cs Ä‘Äƒng kĂ½ ApplicationDbContext báº±ng Npgsql:

    GetConnectionString("DefaultConnection")
    -> fallback Configuration["SUPABASE_CONNECTION_STRING"]
    -> normalize PostgreSQL URI/keyword format
    -> AddDbContext<ApplicationDbContext>(UseNpgsql)

Configuration precedence thá»±c táº¿ cá»§a WebApplication.CreateBuilder(args):

    appsettings.json
    -> appsettings.{Environment}.json
    -> User Secrets (Development)
    -> environment variables
    -> command-line arguments

Evidence:

- services/api/appsettings.json cĂ³ ConnectionStrings:DefaultConnection rá»—ng, khĂ´ng chá»©a credential.
- KhĂ´ng cĂ³ appsettings.Development.json trong API project.
- RestaurantPOS.api.csproj cĂ³ UserSecretsId.
- Source há»— trá»£ ConnectionStrings:DefaultConnection, SUPABASE_CONNECTION_STRING, Jwt:Secret, Seed:AdminPassword vĂ  cĂ¡c key HTTPS.
- KhĂ´ng cĂ³ localhost database fallback trong source.

## 2. Current configuration source

| Source | Status | Evidence |
|---|---|---|
| Tracked config | PRESENT, empty placeholder | services/api/appsettings.json |
| User Secrets | PRESENT | Secure configuration keys tá»“n táº¡i; values khĂ´ng ghi vĂ o report |
| Environment variable | SUPPORTED | ConnectionStrings__DefaultConnection, SUPABASE_CONNECTION_STRING, seed fallback |
| Unknown external override | POSSIBLE | ASP.NET command-line/config providers |

Provider theo source: PostgreSQL qua Npgsql, vá»›i contract/configuration ghi rĂµ Supabase PostgreSQL. KhĂ´ng network check nĂªn khĂ´ng xĂ¡c nháº­n reachability hoáº·c database instance hiá»‡n táº¡i.

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

EnsureCreated, EnsureDeleted vĂ  InitializeDatabase khĂ´ng Ä‘Æ°á»£c dĂ¹ng trong API startup. EnsureCreated chá»‰ xuáº¥t hiá»‡n trong tests/RestaurantPOS.Tests/TestDbContextFactory.cs vá»›i SQLite in-memory.

## 4. Migration safety

- db.Database.Migrate() Ä‘Æ°á»£c gá»i unconditional, khĂ´ng cĂ³ IsDevelopment gate.
- Behavior nĂ y Ă¡p dá»¥ng cho má»i environment khi API startup.
- Migration/seed exception Ä‘Æ°á»£c wrap thĂ nh InvalidOperationException, API khĂ´ng tiáº¿p tá»¥c serve.
- Náº¿u connection trá» remote Supabase, startup cĂ³ thá»ƒ mutate database báº±ng migration pending.

Migration history cĂ³ destructive operations (DropTable, DropColumn, raw SQL), gá»“m:

- 20260919075618_Removeremoved staff-finance moduleAndRepairReceiptSettings trong migration tree bá»• sung, cĂ³ drop removed staff-finance module-related tables/columns;
- cĂ¡c migration corrective/repair cÅ© cĂ³ drop/recreate operations;
- lá»‹ch sá»­ váº«n chá»©a removed staff-finance module migrations nhÆ° 20260906143000_Addremoved staff-finance module, 20260907060600_AddMissingremoved staff-finance modulesAndSettings vĂ  20260907155909_RepairMissingDatabaseSchema.

KhĂ´ng cháº¡y migration nĂªn khĂ´ng káº¿t luáº­n database hiá»‡n táº¡i Ä‘Ă£ apply migration nĂ o.

### Split migration trees â€” HUMAN REVIEW

CĂ³ hai cĂ¢y migration source:

- services/api/src/Infrastructure/Persistence/Migrations, namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;
- services/api/Infrastructure/Persistence/Migrations, namespace RestaurantPOS.api.Infrastructure.Persistence.Migrations.

ApplicationDbContext náº±m trong RestaurantPOS.Infrastructure.Persistence, cĂ²n SDK project máº·c Ä‘á»‹nh compile .cs trong project tree. ÄĂ¢y lĂ  migration discovery/history risk; khĂ´ng tá»± chá»n, xĂ³a hoáº·c há»£p nháº¥t cĂ¢y migration trong audit.

## 5. Seed safety

Seeder: services/api/src/Infrastructure/Persistence/DbInitializer.cs.

- SeedAsync Ä‘Æ°á»£c gá»i sau Migrate á»Ÿ má»i environment.
- Main seed block cháº¡y khi !Branches.AnyAsync().
- Táº¡o branch chĂ­nh, areas, tables tráº¡ng thĂ¡i trá»‘ng vĂ  products cÆ¡ báº£n.
- Náº¿u chÆ°a cĂ³ employee, default admin chá»‰ Ä‘Æ°á»£c táº¡o á»Ÿ Development.
- Admin password láº¥y tá»« Seed:AdminPassword, fallback RESTAURANTPOS_SEED_ADMIN_PASSWORD.
- Thiáº¿u seed password khi cáº§n táº¡o admin sáº½ throw.
- KhĂ´ng tháº¥y Customer, Order, Payment hoáº·c Kitchen request seed trong DbInitializer.
- Seeder khĂ´ng overwrite rows khi branch Ä‘Ă£ tá»“n táº¡i, nhÆ°ng khĂ´ng idempotent theo tá»«ng entity: náº¿u branch tá»“n táº¡i mĂ  product/table thiáº¿u thĂ¬ outer gate khĂ´ng tá»± bĂ¹.
- KhĂ´ng cĂ³ default password literal trong source.

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

KhĂ´ng táº¡o dá»¯ liá»‡u trong audit nĂ y.

## 7. Safe environment options

### A. Current Supabase database

Setup effort tháº¥p, compatibility cao, nhÆ°ng risk cao/unknown vĂ¬ cĂ³ unrestricted startup migration/seed vĂ  chÆ°a xĂ¡c nháº­n disposable. KhĂ´ng phĂ¹ há»£p E2E hiá»‡n táº¡i.

### B. Separate Supabase development/test project

Setup effort moderate, risk tháº¥p náº¿u project/user/database riĂªng, compatibility cao nháº¥t vá»›i Npgsql/Supabase hiá»‡n táº¡i, migration/seed há»— trá»£ Ä‘áº§y Ä‘á»§. PhĂ¹ há»£p nháº¥t cho E2E.

### C. Local PostgreSQL

Setup effort moderate/high vĂ¬ Docker ngoĂ i scope vĂ  pháº£i cĂ i PostgreSQL thá»§ cĂ´ng; risk tháº¥p náº¿u database/user riĂªng; Npgsql compatibility cao nhÆ°ng khĂ¡c má»™t sá»‘ operational behavior cá»§a Supabase. Fallback tá»‘t.

## 8. Recommended approach

Khuyáº¿n nghá»‹ B â€” separate Supabase development/test project:

1. Giá»‘ng provider/runtime hiá»‡n táº¡i.
2. KhĂ´ng áº£nh hÆ°á»Ÿng dá»¯ liá»‡u hiá»‡n cĂ³.
3. Cho phĂ©p migration/seed vĂ  test order/payment trĂªn database disposable.
4. KhĂ´ng cáº§n Docker.

Local PostgreSQL lĂ  phÆ°Æ¡ng Ă¡n thay tháº¿ náº¿u khĂ´ng thá»ƒ táº¡o Supabase project riĂªng; source khĂ´ng tá»± fallback localhost nĂªn pháº£i cáº¥u hĂ¬nh rĂµ rĂ ng.

## 9. User Secrets templates

CĂ¡c command sau chá»‰ lĂ  template, khĂ´ng cháº¡y trong audit:

    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<SAFE_DEVELOPMENT_POSTGRES_CONNECTION_STRING>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:Secret" "<RANDOM_DEVELOPMENT_JWT_SECRET>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:Issuer" "RestaurantPOS" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:Audience" "RestaurantPOS" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Jwt:ExpiryDays" "7" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "Seed:AdminPassword" "<DEVELOPMENT_ONLY_ADMIN_PASSWORD>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "HTTPS_CERT_PATH" "<OPTIONAL_CERT_PATH>" --project .\services\api\RestaurantPOS.api.csproj
    dotnet user-secrets set "HTTPS_CERT_PASSWORD" "<OPTIONAL_CERT_PASSWORD>" --project .\services\api\RestaurantPOS.api.csproj

CĂ¡c secret runtime thá»±c táº¿ lĂ  SUPABASE_CONNECTION_STRING vĂ  RESTAURANTPOS_SEED_ADMIN_PASSWORD. AI hiá»‡n dĂ¹ng Mock AI local nĂªn khĂ´ng cáº§n provider key cho QR/POS/Kitchen/Payment hoáº·c chat local.

## 10. Startup hardening recommendations

### REQUIRED BEFORE E2E

1. Confirm/create isolated development database.
2. Resolve/review split migration trees and intended migration assembly.
3. Review destructive pending migrations against isolated database only.
4. Configure development JWT, connection and seed password securely.

### RECOMMENDED HARDENING

1. Gate automatic migration behind explicit Development-only opt-in hoáº·c chuyá»ƒn sang controlled migration command.
2. KhĂ´ng auto-migrate production/shared startup; migration pháº£i Ä‘Æ°á»£c review/deploy riĂªng.
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
    YES / UNKNOWN SAFETY â€” Supabase contract; no network check

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



