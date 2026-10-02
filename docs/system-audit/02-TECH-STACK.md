# Actual Technology Stack

| Technology | Version/evidence | Purpose |
|---|---|---|
| .NET / ASP.NET Core | `net8.0` in `services/api/RestaurantPOS.api.csproj` | HTTP API and hosting |
| C# | Project source | Backend |
| EF Core | 8.0.0 | ORM and migrations |
| PostgreSQL/Npgsql | Npgsql EF provider 8.0.0 | Relational persistence |
| JWT Bearer | `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0 | Authentication |
| Swagger | Swashbuckle 6.4.0 | API description/UI in Development |
| React | 18.2.0 in both app package files | Web UIs |
| TypeScript | ^5.0.2 admin/customer | Frontend language |
| Vite | ^4.4.5 | Frontend bundling/dev server |
| TailwindCSS | ^3.3.3 | Styling |
| React Router | ^7.18.3 admin/customer | Client routing |
| SignalR client | ^10.0.11 admin | Kitchen realtime client |
| Recharts/Lucide | package evidence | Charts/icons |
| Google Gemini HTTP API | `Gemini:Model`, `GeminiService` | AI generation and function calling |
| Docker | Removed from current repository | Not used by current runtime/deployment |
| HTTPS certificate | `.https`, `scripts/setup-https.ps1` | Local HTTPS on port 5000 |

The repository does not contain a solution file. It has one backend web project and one test project plus two independent frontend projects. `packages/shared` contains OpenAPI-generated/shared TypeScript types, while each app has its own package lock.
