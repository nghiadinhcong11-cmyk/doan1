# Documentation Conflicts

| Topic | Old documentation says | Source code says | Decision | Evidence |
|---|---|---|---|---|
| Manager role | Some older context says manager is not implemented | `Employee.Role`, login, controller attributes and frontend checks include `manager` | Treat manager as implemented/branch-scoped, with coverage to verify | `Employee.cs`, `AuthController.cs`, controllers, admin app |
| Architecture | Older docs call it strict Clean Architecture | One backend project compiles layered folders; no project-level dependency enforcement | Describe as layered Clean-Architecture-like, not strict enforcement | `RestaurantPOS.api.csproj`, `Program.cs`, folder layout |
| AI | Older docs may describe a broad agent | Current code has Gemini orchestrator + registered function tools, not unrestricted autonomy | Call it tool-calling AI assistant | `AiOrchestrator.cs`, `AiToolRegistry.cs`, `IAiTool.cs` |
| Payment | Older material mentions broader payment/QR capabilities | Current controller accepts cash and bank transfer and records manual completion; no webhook | Document only verified manual methods | `OrderController.cs`, POS page |
| Payroll | Some documents describe payroll as stable/production-ready | Payroll is explicitly out of current scope; obsolete source stubs/UI were removed, while migration history may retain old schema references | Do not document Payroll as a current feature; preserve migration/archive history | Current source, migrations, cleanup report |
| Inventory | Design docs may describe inventory | No verified active Ingredient/Inventory entity/controller/frontend flow | Planned/unknown | active entities/controllers and design docs |
| Customer login | Older docs may imply full account login | Customer token is phone-based; guest token is possible | Document phone token + guest flow | `AuthController.cs`, customer app |
| QR | Older docs may imply QR contains branch/table securely | QR scanner primarily extracts `tableId`; branch is resolved from table response | Document tableId-based flow and server validation requirement | `QRScan.tsx`, `DigitalMenu.tsx`, `TableController.cs` |
| Realtime | Older docs may imply SignalR-only delivery | KDS uses SignalR plus 8-second polling fallback | Document hybrid realtime/recovery behavior | `KitchenPage.tsx`, `KitchenHub.cs` |
| Branch isolation | Older docs claim universal branch isolation | Major order/expense/insight paths check claims, but shared catalog entities lack consistent BranchId | State enforced where evidenced, not universal | `ApplicationDbContext.cs`, controllers |
| Deployment | Older docs may claim production/Supabase/Render | Docker files are removed; direct ASP.NET/Vite execution and configured PostgreSQL/Supabase connection remain; active external deployment is not proven | Mark Docker removed and production unknown | `Program.cs`, frontend package/config files |
| Secrets | Older docs may describe environment-only configuration | `appsettings.json` contains a plaintext-looking Gemini value | Record redacted Critical finding | `services/api/appsettings.json` |
