# First Development Database Migration Validation

Date: 2026-09-30  
Scope: isolated Supabase development database only  
Database target: authorized isolated DEV project ref `qfkgjxwbshjgsxsvkpkp`  
Historical note: `vyphnxzunqthjfypxcab` was an incorrect previously assumed reference and is not an authorized target.  
Execution path: Development API startup with automatic migration explicitly enabled

## Safety pre-flight

- The effective connection configuration came from User Secrets; no connection value was printed.
- The connection target was verified to contain the expected development project reference.
- `ASPNETCORE_ENVIRONMENT=Development` was set for the API process.
- `DatabaseSafety:AllowAutomaticMigration=true` was set for the process.
- TCP connectivity to the Supabase pooler was available on the configured port.
- No current/main database target was used.

Result: `DEV DATABASE TARGET VERIFIED: YES`.

## Execution result

The controlled startup path was used once:

```text
Development
→ Database.Migrate()
→ DbInitializer.SeedAsync()
```

The API reached `Database.Migrate()`, but the PostgreSQL connection failed during the SSL handshake before EF Core could read `__EFMigrationsHistory`.

Sanitized error:

```text
NpgsqlException: Exception while performing SSL handshake
AuthenticationException: Authentication failed
Win32Exception: No credentials are available in the security package
```

The configured connection metadata was checked without exposing values: SSL mode is `Require`, trust-server-certificate is enabled, and no client SSL certificate/key fields are configured. The failure is therefore an environment/client TLS connection blocker, not a reported migration SQL failure.

Per the failure policy, the database was not reset and the migration was not retried. Because the failure occurred while opening the connection, this attempt did not apply migrations or seed data.

## Validation status

The following checks could not be executed because the database connection did not complete:

- `__EFMigrationsHistory` count and latest migration;
- `ReceiptSettings` existence;
- `BusinessInsights` existence;
- final absence of Payroll tables;
- `Employees.BasicSalary` and `Employees.EmployeeType`;
- operational table checks;
- seed data verification.

No database mutation was observed from this attempt.

## API and seed status

- API startup: `FAIL` — startup aborted in the guarded migration/seed block.
- Automatic migration: `ENABLED` for this isolated Development process only.
- Seed: `NOT RUN` because migration connection failed first.
- Full QR/POS/Kitchen/Payment E2E: `NOT RUN`.

## Regression verification

No source changes were made during this validation attempt. The existing backend test suite was run using an isolated output directory:

```text
Passed: 177
Failed: 0
Skipped: 0
Total: 177
```

## Required human action

Resolve the Windows/.NET-to-Supabase pooler SSL handshake issue without changing the target project or exposing credentials. Then rerun this validation once, after re-verifying the project reference. Do not reset the database or edit historical migrations as a response to this TLS failure.

Likely investigation areas are the local .NET/Npgsql TLS environment, the Supabase connection variant/port, and the development machine security provider. Any connection-string adjustment must remain limited to the isolated development secret and must not be committed.

## Final status

```text
DEV DATABASE TARGET VERIFIED: YES
EXPECTED PROJECT REF: qfkgjxwbshjgsxsvkpkp
DATABASE CONNECTION USED: YES
DATABASE MUTATED: NO
ENVIRONMENT: Development
AUTOMATIC MIGRATION: ENABLED
MIGRATION EXECUTION: FAIL
MIGRATION HISTORY COUNT: NOT READ (SSL handshake failure)
LATEST MIGRATION: NOT READ
RECEIPT SETTINGS: NOT VALIDATED
BUSINESS INSIGHT: NOT VALIDATED
PAYROLL FINAL SCHEMA: NOT VALIDATED
BASIC SALARY: NOT VALIDATED
EMPLOYEE TYPE: NOT VALIDATED
SEED: NOT RUN
API STARTUP: FAIL
BACKEND TESTS: 177/177
EMPTY DATABASE → CURRENT SCHEMA: NOT VERIFIED
SAFE FOR DEV E2E: NO
CURRENT/MAIN SUPABASE MUTATED: NO
NEXT ACTION: Resolve the isolated DEV Supabase SSL handshake, re-verify the project ref, then rerun the single controlled Development migration/seed validation.
```

No commit or push was performed.

---

## Current host-execution readiness (2026-10-01)

The normal Windows PowerShell HostSelect1 run verified the authorized isolated DEV target and read-only connectivity:

```text
TARGET CHECK: PASS
TARGET PROJECT REF: qfkgjxwbshjgsxsvkpkp
CONNECTION TARGET: aws-0-ap-southeast-2.pooler.supabase.com:5432/postgres
CONNECTION OPEN: PASS
SELECT 1: PASS
```

This confirms host TCP, TLS, PostgreSQL authentication, and `SELECT 1`. It does not execute or prove migration/seed success.

### Host migration readiness

Startup remains guarded by both conditions:

```text
ASPNETCORE_ENVIRONMENT=Development
DatabaseSafety:AllowAutomaticMigration=true
```

Only when both conditions are true does startup call:

```text
Database.Migrate()
DbInitializer.SeedAsync()
```

Task 37 is therefore prepared for a human-run normal Windows PowerShell migration, but remains pending until that run completes. No migration or seed has succeeded in this report.

Npgsql 8.0.0 continues to emit `NU1903`; no package upgrade is part of Task 37. Track that as a separate follow-up dependency/security task after migration and E2E stabilization.

```text
AUTHORIZED DEV PROJECT REF: qfkgjxwbshjgsxsvkpkp
HOST SELECT 1: PASS
MIGRATION EXECUTION: PENDING HUMAN HOST RUN
SEED EXECUTION: PENDING HUMAN HOST RUN
API STARTUP AFTER MIGRATION: PENDING HUMAN HOST RUN
TASK 37: NOT PASS YET
DATABASE MUTATED BY CODEX: NO
```

---

## Rerun — Session Pooler (2026-10-01)

The development connection was changed by the human to the Supabase Session Pooler. The authorized project reference is `qfkgjxwbshjgsxsvkpkp`; the configured port was verified as `5432`. The previous Transaction Pooler configuration was not used. The earlier `vyphnxzunqthjfypxcab` reference was incorrect and is not authorized.

### Rerun pre-flight

- Development target reference: verified without printing the connection string.
- Pooler type: Session Pooler.
- Port: `5432`.
- SSL mode: `Require`.
- `ASPNETCORE_ENVIRONMENT`: Development.
- `DatabaseSafety:AllowAutomaticMigration`: `true` for the controlled process.
- TCP connection: `PASS`.
- Existing API process before run: none.

### Rerun connection result

The same controlled startup path was attempted once:

```text
Development
→ Database.Migrate()
→ DbInitializer.SeedAsync()
```

TCP connectivity to the Session Pooler succeeded, but the TLS handshake failed before PostgreSQL authentication and before EF Core could read `__EFMigrationsHistory`.

Sanitized runtime result:

```text
NpgsqlException: Exception while performing SSL handshake
System.Security.Authentication.AuthenticationException: Authentication failed
Win32Exception 0x8009030E: No credentials are available in the security package
```

Runtime context:

```text
.NET SDK: 10.0.200
Target framework: net8.0
Npgsql EF provider: 8.0.0
OS: Windows win-x64
```

### Rerun classification

```text
TCP CONNECTION: PASS
TLS: FAIL
POSTGRES AUTHENTICATION: NOT REACHED
MIGRATION EXECUTION: FAIL / NOT REACHED
SEED: NOT RUN
API STARTUP: FAIL
DATABASE MUTATED: NO
```

The failure is consistent with a local Windows/.NET Schannel or Npgsql SSL negotiation problem. It is not evidence of a migration SQL error, duplicate `ReceiptSettings` operation, migration ID collision, or PostgreSQL authentication rejection. No migration source was changed and no database reset or repeated retry was performed.

### Final rerun summary

```text
DEV DATABASE TARGET VERIFIED: YES
EXPECTED PROJECT REF: qfkgjxwbshjgsxsvkpkp
POOLER: SESSION
PORT: 5432
TCP CONNECTION: PASS
TLS: FAIL
POSTGRES AUTHENTICATION: FAIL / NOT REACHED
DATABASE CONNECTION USED: YES (connection attempt only)
DATABASE MUTATED: NO
MIGRATION EXECUTION: FAIL
MIGRATION HISTORY COUNT: NOT READ
LATEST MIGRATION: NOT READ
RECEIPT SETTINGS: NOT REACHED
BUSINESS INSIGHT: NOT REACHED
PAYROLL FINAL SCHEMA: NOT REACHED
BASIC SALARY: NOT REACHED
EMPLOYEE TYPE: NOT REACHED
SEED: NOT REACHED
API STARTUP: FAIL
BACKEND TESTS: 177/177 (previous non-DB regression run)
EMPTY DATABASE → CURRENT SCHEMA: NOT VERIFIED
SAFE FOR DEV E2E: NO
CURRENT/MAIN SUPABASE MUTATED: NO
NEXT ACTION: Resolve the local Windows/.NET Npgsql TLS handshake failure, then perform one new target pre-flight and rerun the controlled Development migration path.
```

The required next fix is environmental/client-side: verify the local Schannel/TLS provider and the installed .NET/Npgsql runtime path, or use a supported PostgreSQL client/runtime configuration for the isolated development connection. Do not disable SSL, use `SSL Mode=Disable`, modify migration history, or run the full E2E flow until this connection blocker is resolved.

---

## Current host startup evidence (human-run)

The human started the API from normal Windows PowerShell with:

```text
ASPNETCORE_ENVIRONMENT=Development
DatabaseSafety__AllowAutomaticMigration=true
```

Observed output included:

```text
Insight Background Service is starting.
Now listening on: https://[::]:5000
Application started.
Hosting environment: Development
```

No startup migration/seed exception was observed. Record the startup pipeline as completed without startup exception, but do not treat this alone as proof that every migration, schema object, or seed category is correct.

```text
AUTHORIZED DEV PROJECT REF: qfkgjxwbshjgsxsvkpkp
POOLER: SESSION
PORT: 5432
HOST TCP: PASS
HOST TLS: PASS
POSTGRES AUTH: PASS
HOST SELECT 1: PASS
DATABASE CONNECTION USED: YES
DATABASE MUTATED: YES
STARTUP MIGRATION/SEED PIPELINE: COMPLETED WITHOUT STARTUP EXCEPTION
API STARTUP: PASS
MIGRATION HISTORY: PENDING READ-ONLY VALIDATION
SCHEMA: PENDING READ-ONLY VALIDATION
SEED: PENDING READ-ONLY VALIDATION
EMPTY DATABASE -> CURRENT SCHEMA: PENDING VALIDATION
SAFE FOR DEV E2E: NOT YET
TASK 37: NOT PASS YET
```

The read-only post-migration validator is `scripts/diagnostics/HostSelect1/`. It must be run by a human from normal Windows PowerShell; Codex must not execute it because of the known sandbox Schannel failure. The validator does not run migrations, seed, or writes.
