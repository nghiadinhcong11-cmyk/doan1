# Security Audit

Severity is based on repository evidence, not an external penetration test.

| Severity | Finding | Evidence/impact | Recommendation |
|---|---|---|---|
| Critical | Plaintext-looking Gemini API key in config | `services/api/appsettings.json`; credential exposure if real or committed | Revoke/rotate, remove from tracked config/history, use environment/user secrets |
| High | Legacy plaintext password comparison remains | `AuthController.Login` and change-password fallback compare stored value directly | Migrate all rows to hashes and remove fallback after migration/verification |
| High | Payment completion broadcasts with `Clients.All` | `OrderController.PayOrder`; other branches/users can receive payment payload | Use authorized branch groups and verify frontend consumers |
| High | CORS policy is named `AllowAll` and allows dynamic HTTPS ports 5173/5174 in Development | `Program.cs`; broad origin acceptance within allowed scheme/ports | Rename and restrict by explicit environment/configured origins; test SignalR requirements |
| Medium | Branch ownership is inconsistent for shared catalog entities | Product/Area/Topping/Promotion models do not consistently carry BranchId | Decide shared-vs-branch policy and enforce it server-side |
| Medium | AI permission gate does not itself enforce branch scope | `AiPermissionService` delegates branch safety to tools/services | Enforce context/resource branch policy centrally or prove each tool filter |
| Medium | Customer/QR identifiers are client supplied | `QRScan`, customer endpoints and menu flow | Validate table active/branch ownership server-side for every mutation; add anti-abuse controls |
| Medium | Broad exception message return in password change | `AuthController.ChangePassword` returns `ex.Message` | Log safely server-side and return generic client messages |
| Low | Developer exception page/Swagger enabled in Development | `Program.cs` | Ensure production environment cannot be misconfigured as Development |
| Low | Frontend role and branch values in localStorage are UX state | admin app reads `userRole`, `selectedBranchId` | Keep backend as security boundary; avoid treating localStorage as authorization |

Positive controls evidenced: JWT issuer/audience/signature/lifetime validation, `[Authorize]` attributes, branch helper checks in major order/expense/insight paths, rate limits for auth/AI/orders, password hashing service, parameterized EF queries, and SignalR authorization.
