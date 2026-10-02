# Architecture

## Backend

The physical layout is Domain entities, Application services/DTOs/interfaces, Infrastructure persistence/services, WebAPI controllers/hubs, and an `AI` module. `Program.cs` is the composition root and registers services directly. This is a layered Clean-Architecture-like arrangement; the repository does not prove strict project-level dependency enforcement because the backend is a single `.csproj` compiling source folders together.

Runtime dependency:

```text
Browser -> Controller/AI Controller -> Application Service or AI Tool -> ApplicationDbContext/Infrastructure -> PostgreSQL
                                             \-> SignalR notifier -> KitchenHub clients
```

`AiOrchestrator` calls registered tools; tools are intended to call application services. `Program.cs` also registers `DbContext`, JWT, CORS, rate limiting, SignalR, Swagger, hosted insight generation, and the Gemini HTTP client.

## Frontend

`apps/admin-web` is the staff/admin/POS/KDS application organized by feature. `apps/customer-web` is a separate customer app with QR scan, digital menu, customer profile/login, and reservations. Both use browser `fetch` and local storage for token/UI context; there is no repository-wide React Query or state-management library evidence.

## Hosting and middleware

The API listens on HTTPS port 5000, runs migrations and seeding at startup, maps controllers and `/kitchenHub`, enables Swagger only in Development, uses CORS policy `AllowAll` with configured origins plus development HTTPS LAN-origin logic, rate limiting, authentication and authorization. Developer exception page is enabled in Development.

## Duplicate/legacy layout observation

Current migrations are under `services/api/src/Infrastructure/Persistence/Migrations`. A separate top-level `services/api/Infrastructure/Persistence/Migrations` contains newer-looking migration files, and `services/Migrations_backup_20260919/` contains a backup migration set. These are not treated as the active model without further build/database verification.
