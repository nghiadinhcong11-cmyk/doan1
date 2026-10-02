# Password and Authentication Hardening Audit

Audit date: 2026-09-30

Scope: read-only audit of the current source, configuration, frontend consumers, tests and existing security documentation. No source, UI, database or migration was changed.

## Executive conclusion

The system uses ASP.NET Core Identity `PasswordHasher<T>` for Employee and Customer credential creation/update and JWT Bearer for API authentication. It does not use BCrypt in the inspected source.

The main high-risk issue is a live plaintext compatibility fallback in `AuthController.Login` and `AuthController.ChangePassword`. The fallback performs direct string comparison and automatically replaces the stored value with a hash after successful use. This is both a legitimate legacy migration mechanism and a security vulnerability while plaintext values remain accepted.

The repository does not prove that production data still contains plaintext credentials. Therefore the safe decision is: do not remove the fallback blindly; first verify/migrate legacy rows, then remove it. The current customer authentication path is a separate concern: `customer-token` authenticates by phone number/guest flow and does not verify Customer.Password.

## 1. Authentication inventory

### Employee account

Entity: `RestaurantPOS.Domain.Entities.Employee` with `Username`, `Password`, `Role`, `BranchId`, `IsActive`.

Authentication: `POST /api/Auth/login` in `AuthController.Login` queries an active Employee by username, verifies the password, then issues a JWT. The request `Mode` controls the application surface:

| Role | Account/entity | Current login path | Token behavior |
|---|---|---|---|
| admin | Employee | `mode=admin` | `admin`, branch claims may be present |
| manager | Employee | `mode=admin` | `manager`, assigned branch claims |
| cashier | Employee | `mode=cashier` | `cashier`, assigned branch claims |
| kitchen | Employee | `mode=kitchen` | `kitchen`, assigned branch claims |
| employee | Employee | No dedicated accepted login mode is evidenced in `Login`; role is used by authorization and other JWT-based flows | No employee-mode login path is exposed by this controller |

The login method requires `Employee.IsActive == true`. Admin/manager/cashier/kitchen mode checks are enforced after password verification. Branch selection is checked for cashier and kitchen when a BranchId is supplied; admin is exempt from the mismatch check.

### Customer account

Entity: `RestaurantPOS.Domain.Entities.Customer` with a `Password` field, but the normal current customer authentication is not password login.

`POST /api/Auth/customer-token` looks up the phone number and directly issues a `customer` JWT for an existing customer. If no customer exists, it issues a guest-like customer JWT with a generated identifier and no Customer row. No password is requested or verified in this flow.

Customer creation is `POST /api/Customer`, which is publicly allowed for a new phone number and hashes a supplied password. Customer profile update can hash `NewPassword`. However, no customer-password login endpoint was found, so the stored Customer.Password is not the credential used by `customer-token`.

### Register/create account

- Employee account creation: authorized `POST /api/Employee`; admin/manager can create employees and the controller hashes a supplied password.
- Customer creation: public new-customer path in `CustomerController.CreateOrUpdate`; supplied password is hashed.
- No separate `/register`, `/reset-password`, email reset, OTP reset or administrator reset endpoint was found.
- No automatic account-wide password reset or token revocation flow was found.

### Change password

`POST /api/Auth/change-password` is `[Authorize]` and requires the request Id to equal the JWT NameIdentifier. It accepts either Employee or Customer depending on the JWT role and request Type. It verifies the old password, including the legacy fallback, and hashes the new password with the appropriate `PasswordHasher<T>`.

Customer profile PATCH also supports `NewPassword` and hashes it, but this path is protected by customer/admin/manager authorization and does not verify the old password.

### Seed/default account

`DbInitializer` creates an admin Employee when no employees exist and hashes a hardcoded default credential before saving. The credential value is intentionally not repeated in this report. This is a default-account exposure and operational risk even though the stored database value is hashed.

`Customer.Password` has a hardcoded default value in the entity. New customer creation can therefore receive a predictable default credential before hashing when no password is supplied, although the current `customer-token` path does not verify it.

### Legacy migration

A successful legacy login or change-password operation sets `needsUpgrade`/rewrites the credential with `HashPassword`. This is an opportunistic migration, not a complete database migration: untouched accounts remain potentially plaintext.

## 2. Password storage and hashing

| Operation | Implementation | Evidence |
|---|---|---|
| Employee hash creation | `IPasswordHasher<Employee>.HashPassword` | `Program.cs`, `EmployeeController.cs`, `DbInitializer.cs` |
| Customer hash creation | `IPasswordHasher<Customer>.HashPassword` | `Program.cs`, `CustomerController.cs`, `AuthController.cs` |
| Employee hash verification | `VerifyHashedPassword` | `AuthController.Login`, `AuthController.ChangePassword` |
| Customer hash verification | `VerifyHashedPassword` | `AuthController.ChangePassword` only; not used by `customer-token` |
| Rehash | `PasswordVerificationResult.SuccessRehashNeeded`, followed by `HashPassword` | `AuthController.Login` |
| Stored credential field | `Employee.Password`, `Customer.Password` | `Employee.cs`, `Customer.cs` |

`PasswordHasher<T>` is the ASP.NET Core Identity implementation. No BCrypt package or BCrypt call is present. No custom `PasswordHasherOptions` is configured. The default ASP.NET Core Identity V3 format is PBKDF2-based; the exact work factor is therefore framework-version behavior, not an application-defined policy.

## 3. Plaintext legacy fallback

### Employee login fallback

File/method: `services/api/src/WebAPI/Controllers/AuthController.cs`, `Login`.

1. The controller first calls `_employeeHasher.VerifyHashedPassword`.
2. A malformed/non-hash stored value can cause `FormatException`; the catch leaves `passwordValid` false.
3. It then evaluates `employee.Password == request.Password` directly.
4. If equal, login succeeds and `needsUpgrade` is set.
5. The plaintext value is replaced with a newly generated hash and saved.

### Employee/customer change-password fallback

File/method: `AuthController.ChangePassword`.

- Employee path catches `FormatException`, then compares `employee.Password == request.OldPassword`.
- Customer path catches `FormatException`, then compares `customer.Password == request.OldPassword`.
- On success, the new password is hashed and saved.

### Classification

| Aspect | Finding |
|---|---|
| Legacy migration mechanism | Opportunistic rehash on successful legacy login/change-password |
| Security vulnerability | Plaintext equality comparison accepts a plaintext database value as a valid credential |
| Account types affected | Employee login; Employee and Customer change-password; Customer does not use password for `customer-token` login |
| Direct plaintext comparison | PRESENT in `AuthController.cs` |
| Automatic rehash | PRESENT after successful fallback |
| Evidence that current production DB has plaintext rows | UNKNOWN; source and tests prove support, not live data state |
| Existing fallback tests | PRESENT: `PasswordHashingTests.Employee_Login_UpgradesPlaintextToHash`; change-password tests also seed plaintext values |
| Risk of immediate removal | Any Employee with a non-hash/plaintext Password would be unable to log in; legacy change-password would also fail. Customer password rows are not used by customer-token login, but password-change compatibility could break. |

The current tests intentionally validate the fallback, so removing it without replacing those tests with a migration/verification strategy would be a breaking behavior change.

## 4. Password exposure audit

### Positive controls

- `EmployeeController` explicitly nulls Employee.Password before returning employee records.
- `CustomerController.MapToDto` does not include Customer.Password.
- Passwords are submitted in request bodies, not query strings, in inspected frontend flows.
- JWT claims do not contain passwords.
- No password logging statement was found in the inspected source.

### Findings

| Area | Status | Evidence/impact |
|---|---|---|
| API response DTOs | No direct password field found | Employee responses clear Password; Customer DTO omits it |
| Frontend state | Temporary plaintext exists | Login/profile forms hold password in React state while editing; no evidence of localStorage/sessionStorage persistence |
| Employee create/update binding | Risk of over-posting | Controller binds the full Employee entity, including Password, rather than a purpose-built credential DTO |
| Exception responses | Potential sensitive detail | Employee create/update return exception and inner-exception messages; ChangePassword returns `ex.Message` |
| Seed credential | EXPOSED in source | A default admin credential is present in `DbInitializer`; it is hashed before persistence but discoverable by anyone with repository access |
| Customer default | EXPOSED/predictable | `Customer.Password` defaults to a predictable value before hashing |
| Gemini API key | EXPOSED | `services/api/appsettings.json` contains a non-empty key-like value; value omitted here; rotation is required if real |
| JWT secret | UNKNOWN | Tracked `appsettings.json` value is empty and startup requires secure external configuration; runtime secret cannot be verified |
| PostgreSQL connection string | UNKNOWN | Tracked value is empty; runtime environment/user secrets cannot be verified |

## 5. Password policy

### Backend

No centralized backend password policy validator was found. Employee creation, Customer creation, Employee change-password, Customer change-password and Customer profile `NewPassword` hashing do not consistently enforce length/character rules.

### Frontend

- Admin `ProfilePage` checks new employee password length of at least 8 before calling change-password.
- The reusable `ChangePassword` component checks confirmation equality but does not establish a matching backend policy.
- Employee creation UI sends a password but no complete strength policy is evidenced.
- Customer profile/password UI does not establish the same minimum/complexity policy.

### Current policy matrix

| Flow | Minimum | Upper/lower/digit/special | Maximum | Backend enforced |
|---|---|---|---|---|
| Employee create | Not established | Not established | Not established | No |
| Customer create | Not established; predictable entity default exists | Not established | Not established | No |
| Employee change | Frontend-only 8 in one screen | Not established | Not established | No |
| Customer change/profile | Not established | Not established | Not established | No |
| Reset password | No endpoint found | N/A | N/A | N/A |

Recommended practical policy for a POS/thesis system: minimum 8 characters, maximum at least 64, reject empty/whitespace and optionally reject the known default seed credential; do not require arbitrary composition rules if a long passphrase is accepted. Enforce the same server-side rule for employee creation, customer creation where password is retained, and password changes.

## 6. Secret audit

| Secret | Status | Evidence |
|---|---|---|
| Gemini API key | EXPOSED / NEEDS ROTATION | Non-empty key-like value in tracked `services/api/appsettings.json`; value intentionally redacted from this report |
| JWT secret | UNKNOWN | Empty tracked setting; required from secure runtime configuration; actual environment/user-secret value not inspected |
| PostgreSQL connection string/password | UNKNOWN | Empty tracked setting; runtime environment/user-secret value not inspected |

The Gemini key should be considered compromised if it is real: revoke/rotate it and remove it from repository history through the project’s approved secret-remediation process. No secret values are reproduced here.

## 7. Authentication/authorization interaction

- JWT uses HMAC-SHA256 with issuer, audience, signature and lifetime validation; configured expiry defaults to 7 days.
- Role, user id and branch claims are embedded in the token.
- Disabled Employee accounts cannot start a new login because Login filters `IsActive`.
- Existing Employee JWTs are not checked against current `IsActive` on every request by the inspected authentication layer; disabling an account therefore does not visibly revoke already-issued tokens.
- Changing a password does not revoke existing JWTs; tokens remain valid until expiry unless another mechanism outside this repository exists.
- Customer `customer-token` does not check `Customer.IsActive` before issuing a token.
- Customer guest tokens use a generated identifier and can be issued without a persisted Customer account.
- No refresh-token, token-revocation list, session store or password-reset token mechanism was found.

## 8. Test coverage

### Present

- Correct hash creation and verification for Employee and Customer: `PasswordHashingTests`.
- Plaintext Employee login upgrade: `PasswordHashingTests.Employee_Login_UpgradesPlaintextToHash`.
- Employee create hashes password and clears response password.
- Customer profile update hashes a new password.
- Correct/incorrect old password and self-only change-password authorization: `ChangePasswordSecurityTests`.
- Cross-account and admin self-change restrictions.
- JWT signature, issuer, audience, expiry, malformed token and missing-role behavior: `JwtAuthenticationTests`.
- Role/branch authorization tests exist across controllers.

### Missing or incomplete

- No explicit AuthController test that Employee login rejects an inactive account.
- No test that a disabled account’s already-issued JWT is rejected, and source indicates no such revocation check.
- No Customer `customer-token` test for inactive Customer behavior.
- No dedicated customer-password login test because no customer-password login endpoint exists.
- No centralized password-policy tests for create/change/profile flows.
- No explicit test for malformed legacy hash versus plaintext fallback boundaries.
- No test proving no password appears in every relevant API response/exception/log path.
- No test for seed default credential rotation/disablement.
- No live migration/data audit test that enumerates plaintext rows without exposing their values.

## 9. Hardening fix plan (no fixes performed)

| ID | Priority | Problem | Risk | Files affected | Recommended change | Migration required? | Tests required |
|---|---|---|---|---|---|---|---|
| AUTH-P0-01 | P0 | Real-looking Gemini key is tracked | External API compromise/cost/data exposure | `services/api/appsettings.json`, secret history | Revoke/rotate, remove from tracked config/history, load only from secret storage | No schema migration | Secret scanning and startup config tests |
| AUTH-P0-02 | P0 | Plaintext fallback accepts direct equality | Credential compromise if DB plaintext is exposed | `AuthController.cs`, data migration/runbook | Inventory credential encoding without logging values; migrate/reset all legacy rows; remove fallback only after verification | No schema migration necessarily; data migration/runbook required | Hashed login, malformed hash rejection, migration completeness, no plaintext fallback |
| AUTH-P1-01 | P1 | Predictable seed/default credentials | Unauthorized first-use access | `DbInitializer.cs`, `Customer.cs` | Remove predictable defaults; require injected one-time setup secret or force first-login change; never document default credential | No schema migration necessarily | Seed behavior and first-login tests |
| AUTH-P1-02 | P1 | No unified backend password policy | Weak/inconsistent credentials | `AuthController.cs`, `EmployeeController.cs`, `CustomerController.cs`, shared validator/DTOs | Add one server-side validator and apply consistently to create/change/profile; align frontend messages later | No schema migration | Policy matrix tests for all flows |
| AUTH-P1-03 | P1 | Customer token bypasses password and active state | Phone possession/guessing acts as authentication; inactive users can receive tokens | `AuthController.cs`, customer flow | Decide explicitly whether phone OTP/guest is intended; if account auth is required, add verified OTP/password flow and enforce `IsActive` | No schema migration expected | Existing/inactive/guest customer-token tests |
| AUTH-P1-04 | P1 | Existing JWTs survive disable/password change | Stolen token remains usable up to expiry | JWT/authentication middleware, token/session design | Add short-lived access tokens plus revocation/version check or server-side session/revocation strategy | Possibly no schema migration; depends on chosen revocation store | Disabled-account token, password-change token invalidation |
| AUTH-P2-01 | P2 | Entity binding accepts credential fields broadly | Accidental credential overwrite/over-posting | Employee/Customer controllers and DTOs | Use dedicated request DTOs; never bind entity Password from general profile payloads | No | DTO/API contract and over-posting tests |
| AUTH-P2-02 | P2 | Exception messages expose internal details | Information disclosure | `AuthController`, `EmployeeController`, `CustomerController` | Return generic client errors; log structured server-side without credentials | No | Error response tests |
| AUTH-P2-03 | P2 | Password exposure/regression coverage incomplete | Future DTO/log changes may leak credentials | API tests | Add response redaction and sensitive-data logging tests | No | Response/log scanning tests |

## 10. Plaintext fallback decision

Decision: **B — migration/verification required before removal**.

Reasons:

1. Source contains active fallback and direct comparisons.
2. Tests prove plaintext legacy accounts are supported and upgraded.
3. Repository does not prove whether the live database contains any legacy rows.
4. Removing fallback now could lock out legacy Employee accounts.

Recommended safe sequence: inventory encoding metadata without exporting passwords → migrate/reset legacy rows through a controlled process → monitor failed legacy-format logins → remove fallback and update tests → verify no plaintext rows remain. Do not log or report password values.

## Final status

- PASSWORD HASHING: **NEEDS CHANGE** — hashing is present and appropriate, but fallback/default credential handling needs hardening.
- PLAINTEXT FALLBACK: **MIGRATION REQUIRED** before removal; live-row verification is still needed.
- PASSWORD EXPOSURE: **FOUND** — tracked Gemini key-like value and predictable seed/default credentials; API response redaction is mostly present.
- PASSWORD POLICY: **INCONSISTENT**.
- SECRETS: **ACTION REQUIRED** — Gemini key rotation; JWT/database runtime values remain unknown.
- TEST COVERAGE: **NEEDS ADDITIONAL TESTS**.
