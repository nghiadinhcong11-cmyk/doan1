# Authentication Hardening - Phase 1 Result

**Audit date:** 2026-09-30  
**Scope:** password lifecycle, customer tokens, inactive accounts, seed credentials, Gemini configuration, and JWT validation.  
**Constraints:** no database migration, no database change, no UI change, no commit, no push.

## 1. Customer-token purpose

`POST /api/Auth/customer-token` is currently a hybrid mechanism:

- For an existing active customer, it creates a customer JWT using the customer's identity. This is the current registered-customer session mechanism; it does not perform password authentication.
- For a guest QR order, it creates a customer-role JWT with a generated identity and no password. This is a guest/table-order session mechanism and must remain available for QR ordering.
- An inactive existing customer is now rejected before token creation.

The distinction is now explicit in the JWT claim `customerSessionType`:

| Session | Claim | Password required | Intended use |
|---|---|---:|---|
| Registered customer | `registered` | No current password login endpoint exists | Customer identity, profile/loyalty/order flows |
| Guest QR customer | `guest` | No | QR menu and guest order flow |

This is not a complete customer authentication system. The existing phone-based token endpoint remains a bearer/session mechanism and should not be described as password login until an explicit customer login/OTP requirement is implemented.

Evidence: `services/api/src/WebAPI/Controllers/AuthController.cs`, `apps/customer-web/src/pages/CustomerLogin.tsx`, `apps/customer-web/src/pages/DigitalMenu.tsx`, `services/api/src/Infrastructure/Services/JwtService.cs`.

## 2. QR guest architecture and authorization

Guest QR ordering is preserved. The guest path still obtains a JWT without a password, and the token remains usable by the existing customer order flow. The guest token is marked separately and does not represent a registered customer row.

The JWT validation path allows an explicitly marked guest session. Registered customer sessions are checked against the current Customer row and active state. Legacy customer tokens without the new claim remain accepted only when no matching customer row is found, for backward compatibility with existing guest sessions. A token explicitly marked `registered` is rejected if its customer row is missing or inactive.

Evidence: `services/api/Program.cs:118-163`, `services/api/src/WebAPI/Controllers/AuthController.cs:254-294`.

## 3. Fixes implemented

- Added centralized server-side password policy: 8-128 characters, non-empty/non-whitespace, without arbitrary composition rules.
- Applied the policy to employee creation/update password changes, customer creation, customer profile password changes, and employee/customer self-service password changes.
- Kept ASP.NET Core `PasswordHasher<T>` for both Employee and Customer.
- Centralized legacy plaintext verification in `LegacyPasswordVerifier`.
- A successful legacy plaintext login now immediately hashes and persists the password; subsequent login uses hashed verification.
- Removed the Customer entity's default plaintext password value.
- Employee login continues to require `IsActive == true`.
- Customer token creation now rejects inactive customers.
- Protected JWT requests now re-check employee/customer active state through `OnTokenValidated`.
- Removed the Gemini API key value from tracked `appsettings.json`.
- Removed the hardcoded admin seed password. Development seeding now requires `Seed:AdminPassword` or `RESTAURANTPOS_SEED_ADMIN_PASSWORD` supplied outside tracked configuration.

Evidence: `services/api/src/Application/Common/Security/PasswordPolicy.cs`, `services/api/src/Application/Common/Security/LegacyPasswordVerifier.cs`, `services/api/src/WebAPI/Controllers/AuthController.cs`, `services/api/src/WebAPI/Controllers/EmployeeController.cs`, `services/api/src/WebAPI/Controllers/CustomerController.cs`, `services/api/src/Infrastructure/Persistence/DbInitializer.cs`, `services/api/appsettings.json`.

## 4. Password hashing status

Password hashing remains ASP.NET Core `PasswordHasher<Employee>` and `PasswordHasher<Customer>`. No BCrypt migration was performed. New passwords are hashed before persistence, and existing hashed passwords are verified through `VerifyHashedPassword`.

`SuccessRehashNeeded` is handled as a successful login followed by persistence of the new hash.

## 5. Legacy plaintext migration status

The fallback was not removed in this phase because the live database was not inventoried and removing it could lock out accounts that still contain legacy plaintext values.

The fallback is now isolated in `LegacyPasswordVerifier`. Its behavior is:

1. Try ASP.NET hashed verification.
2. If the stored value is a legacy/non-hash value, compare only for the login/change-password operation.
3. On success, hash with `PasswordHasher<T>` and persist immediately.
4. Do not log or return the plaintext value.

This is a migration path, not a permanent target. A production rollout still needs a controlled inventory/monitoring plan and a later removal decision.

Evidence: `services/api/src/Application/Common/Security/LegacyPasswordVerifier.cs`, `services/api/src/WebAPI/Controllers/AuthController.cs`, `tests/RestaurantPOS.Tests/PasswordHashingTests.cs`.

## 6. Inactive account protection

| Account | New login/token | Existing protected JWT |
|---|---|---|
| Employee | Blocked when `IsActive` is false | Rejected during JWT validation |
| Registered customer | Customer token blocked when inactive | Rejected during JWT validation |
| Guest QR session | No customer row is required | Preserved for QR ordering |

The active-state check is database-backed and does not require a schema change.

## 7. Password policy

Current backend policy is 8-128 characters, rejecting null, empty, whitespace-only, and too-short values. It is applied consistently to operations that actually receive a new password:

- employee creation/update;
- customer registration/creation;
- employee/customer change-password;
- customer profile password update.

No reset-password endpoint was created because the current system does not expose one. Frontend validation was not changed in this backend-focused phase; backend validation remains authoritative.

## 8. Default credentials

- Customer default password `123456` was removed from the entity initializer.
- Admin seed password is no longer hardcoded in source.
- Empty development databases now require a development-only secret through configuration/user secrets/environment variable.
- Passwords are not returned through DTOs or API responses by the changed paths.

Human action: configure a local development seed secret before starting against an empty development database. Do not place it in tracked files.

## 9. Gemini secret handling

The tracked value at `services/api/appsettings.json:21` is now empty. The application must receive the actual key through environment configuration or .NET User Secrets.

The previously exposed provider key must be manually revoked/rotated by a human at the provider. This report does not claim that rotation has happened.

## 10. JWT invalidation status

### Account disable/inactive state

Implemented without migration: protected requests query the current Employee/Customer active state during JWT validation. Therefore an old token for a disabled employee or inactive registered customer is rejected on the next protected request.

### Password change

Not invalidated immediately. The current schema has no token version/security stamp and the project has no refresh-token/revocation store. Existing tokens can remain valid until their normal expiry. Implementing immediate password-change invalidation requires a deliberate security-stamp/token-version design and likely a schema or persistence change; no migration was created in this phase.

## 11. Tests added/updated

Added `tests/RestaurantPOS.Tests/AuthenticationHardeningTests.cs` covering:

- inactive employee login rejection;
- inactive customer token rejection;
- guest customer token availability without a password;
- short employee password rejection before persistence.

Updated password hashing coverage to verify a legacy plaintext login upgrades the stored value and that a subsequent login succeeds through the hash path. Existing change-password tests were adjusted only to use policy-compliant test passwords.

No Payroll, Docker, or unrelated tests were removed.

## 12. Build/test results

### Baseline before this phase

| Command | Result |
|---|---|
| Root `dotnet build` | Failed: repository root has no solution/project |
| Root `dotnet test` | Failed: repository root has no solution/project |
| Backend project build | Passed |
| Backend tests | Passed: 173/173 |
| Admin `npm run build` | Passed, large-chunk warning |
| Customer `npm run build` | Passed |

### After this phase

| Command | Result |
|---|---|
| Backend `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | Passed |
| Backend `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | Passed: 177/177 |
| Admin `npm run build` | Passed, existing large-chunk warning |
| Customer `npm run build` | Passed |

The root-level build/test commands remain invalid for this repository layout; the project-specific commands are the valid verification commands.

## 13. Database and migration impact

- No migration was created.
- No database was modified or dropped.
- The Customer password initializer change does not require a schema change.
- Active-state JWT validation uses existing Employee/Customer columns.
- Legacy plaintext rows, if any, are migrated opportunistically when the account successfully authenticates.
- A future token-version/security-stamp implementation may require a schema change and must be reviewed separately.

## 14. Remaining security work

1. Decide whether registered customers should authenticate through password, OTP, or another explicit flow instead of the current phone-token endpoint.
2. Inventory legacy credential storage in the live database without exposing values, then remove the plaintext fallback after a controlled migration window.
3. Design immediate password-change token invalidation if required; current old JWTs remain valid until expiry.
4. Add integration tests that exercise the full JWT middleware pipeline, not only controller-level behavior.
5. Synchronize frontend password validation messages with the backend policy.
6. Review existing authentication error detail and logging behavior in a separate security pass.

## 15. Human actions required

- Revoke/rotate the previously exposed Gemini API key at the provider.
- Supply a development-only admin seed secret through User Secrets or environment configuration when initializing an empty development database.
- Decide and document the intended registered-customer authentication model.
- Confirm the acceptable token lifetime and whether password-change revocation is required immediately.

## Final status

| Area | Status |
|---|---|
| CUSTOMER-TOKEN | CHANGED |
| QR ORDERING | PASS |
| PASSWORD HASHING | SAFE - ASP.NET `PasswordHasher<T>` retained |
| PLAINTEXT LEGACY | MIGRATION PATH READY; fallback still temporary |
| INACTIVE ACCOUNT | PROTECTED |
| OLD JWT AFTER DISABLE | INVALIDATED on protected request |
| OLD JWT AFTER PASSWORD CHANGE | STILL VALID until expiry; requires separate design/change |
| GEMINI KEY IN TRACKED CONFIG | REMOVED; provider rotation still required |
| TESTS | 177 passed / 0 failed |
