# Windows / .NET / Npgsql TLS Diagnostic

Date: 2026-10-01  
Scope: read-only connectivity diagnosis for the isolated DEV Supabase project  
Database mutation: none  
Migrations/seed: not executed

## Environment inventory

| Item | Result |
|---|---|
| Windows | Windows 10 Home Single Language, display version 25H2, build 26200.9457 |
| OS architecture | win-x64 |
| .NET SDK | 10.0.200; SDK 9.0.314 also installed |
| .NET runtimes | Microsoft.NETCore.App 8.0.26, 9.0.16, 10.0.4; ASP.NET Core 8.0.26, 9.0.16, 10.0.4 |
| API target | `net8.0` |
| EF Core | 8.0.0 |
| Npgsql EF provider | `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.0 |
| Npgsql runtime assembly | 8.0.0 assembly from the API build output |

The API is targeting .NET 8, while the installed SDK selected for the diagnostic is .NET 10. The .NET 8 runtime is installed and is used by the `net8.0` diagnostic process.

## Connection configuration shape

The effective configuration was read without printing its value. The authorized isolated DEV project is `qfkgjxwbshjgsxsvkpkp`. The previously assumed reference `vyphnxzunqthjfypxcab` was incorrect and is no longer authorized.

| Property | Sanitized result |
|---|---|
| Host | Supabase pooler host; exact host withheld |
| Pooler | Session Pooler |
| Port | `5432` |
| Database | configured; value withheld |
| Username | configured; value withheld |
| Password | present but never printed |
| SSL Mode | `Require` |
| Trust Server Certificate | `true` in the configured shape |
| Client SSL certificate/key | not configured |

The API source does not add PostgreSQL SSL, certificate, integrated-security, Kerberos, or GSS options. Its Npgsql configuration explicitly sets `SslMode=Require` while preserving the other connection-string options.

## Environment and certificate checks

No relevant process, user, or machine variables were present for:

```text
PGSSLMODE
PGSSLROOTCERT
PGSSLCERT
PGSSLKEY
PGSSLNEGOTIATION
PGGSSENCMODE
PGSERVICE
PGSERVICEFILE
Npgsql*
DOTNET_SYSTEM_NET_SECURITY*
```

The default Windows PostgreSQL client directory `%APPDATA%\postgresql` was absent. No `postgresql.crt`, `postgresql.key`, or equivalent default client certificate was detected. There is no evidence that Npgsql unintentionally selected a PostgreSQL client certificate.

No recent Schannel events were available from the accessible System event log for the diagnostic window. No registry or machine TLS policy was changed.

## Minimal Npgsql tests

A temporary `net8.0` diagnostic referenced the existing Npgsql 8.0.0 assembly and opened a connection using the existing DEV secret in process memory. It executed only `SELECT 1` after a successful open; no connection opened successfully.

### Current configuration

```text
CURRENT: FAIL
NpgsqlException: Exception while performing SSL handshake
AuthenticationException: Authentication failed, see inner exception.
Win32Exception 0x80004005: No credentials are available in the security package
```

Failure occurred before PostgreSQL authentication and before `SELECT 1`.

### GSS/SSPI hypothesis

The installed Npgsql 8.0.0 connection-string parser rejected `Gss Encryption Mode=Disable` with:

```text
ArgumentException: Couldn't set gss encryption mode
KeyNotFoundException: The given key was not present in the dictionary.
```

Therefore the requested GSS-disabled experiment is `NOT SUPPORTED` by this installed provider/configuration. There is no evidence that a GSS negotiation was reached. The current failure remains an SSL/TLS Schannel failure.

### Certificate validation hypothesis

A temporary variant using `SSL Mode=VerifyFull` and certificate validation enabled produced the same `SslStream` credential-acquisition error, not a certificate-chain or hostname-validation error. This makes certificate trust validation an unlikely primary cause. The configured `Trust Server Certificate` property is also reported obsolete/no-op by the installed Npgsql 8.0.0 API.

## Execution-environment comparison

The Codex execution environment and a normal Windows PowerShell host do not produce the same result. They must not be classified as one machine-wide failure.

### Codex execution environment

```text
TCP: PASS
TLS: FAIL
ERROR: Schannel SEC_E_NO_CREDENTIALS / 0x8009030E
POSTGRES AUTHENTICATION: NOT REACHED
SELECT 1: NOT REACHED
```

### Normal Windows PowerShell host (human-run evidence)

The HostSelect1 diagnostic was run outside the Codex execution environment using normal Windows PowerShell. The authorized target was verified and the connection completed successfully:

```text
TCP: PASS
TLS: PASS
POSTGRES SERVER: REACHED
POSTGRES AUTHENTICATION: PASS
SELECT 1: PASS
```

This host result proves that TCP, TLS, PostgreSQL authentication, and `SELECT 1` succeed in the normal Windows PowerShell execution context.

## Root-cause classification

### A. Codex execution context

```text
TCP: PASS
TLS: FAIL — Schannel credential acquisition, 0x8009030E
```

This is an execution-context-specific TLS problem. It is not evidence of a general Windows host TLS failure.

### B. Normal Windows PowerShell host

```text
TCP: PASS
TLS: PASS
POSTGRES AUTHENTICATION: FAIL — SQLSTATE 28P01
```

The previous host-side `28P01` result was resolved by correcting the isolated DEV connection credentials/identity. The normal host is no longer blocked.

Evidence for the Codex-context classification:

1. TCP to Session Pooler `5432` passes.
2. Npgsql fails in `SslStream` with Windows error `0x8009030E`.
3. No PostgreSQL authentication is reached in the Codex execution environment.
4. No client certificate, PostgreSQL TLS environment variable, or GSS environment variable was detected.
5. Certificate-validation and current `Require` variants fail identically.
6. The failure is independent of EF Core migrations and occurs in a minimal connection diagnostic.

Evidence for the host classification:

1. Normal Windows PowerShell reached `NpgsqlConnector.AuthenticateSASL(...)`.
2. PostgreSQL returned SQLSTATE `28P01`, which is an authentication failure after TLS.
3. Therefore host TCP, TLS, PostgreSQL authentication, and `SELECT 1` are all `PASS` for the authorized DEV target.

Secondary classification:

```text
G. PACKAGE/VERSION COMPATIBILITY — possible contributor, not proven
```

The project uses Npgsql 8.0.0 with a .NET 8 target on a machine whose selected SDK is .NET 10. This is not by itself proof of incompatibility, but it is a safe investigation point. No package upgrade was performed.

## Fix applied

```text
FIX APPLIED: NONE
```

No SSL was disabled, no User Secret was changed, no Windows policy was modified, and no application source was changed. The host-only diagnostic project was added for manual execution; Codex did not execute it. No database operation was performed by that project.

Recommended human actions, in order:

1. Preserve the verified isolated DEV connection identity. Do not change production credentials.
2. The host-only diagnostic returned `SELECT 1: PASS` from normal Windows PowerShell.
3. Proceed to the controlled host-executed Task 37 migration, with Development and explicit automatic-migration opt-in.

Do not use `SSL Mode=Disable`, do not bypass TLS, and do not run migrations until a read-only Npgsql `SELECT 1` test succeeds.

## Host-only SELECT 1 diagnostic

The repository now contains `scripts/diagnostics/HostSelect1/`, a standalone `net8.0` console diagnostic using Npgsql 8.0.0. It reads `ConnectionStrings:DefaultConnection` from the existing User Secrets store (`RestaurantPOS-api-local`) or from `ConnectionStrings__DefaultConnection`. It never prints credentials, runs no EF Core code, performs no migration/seed, and executes only `SELECT 1` after opening the connection.

Run this command manually from a **normal Windows PowerShell host**, not from Codex:

```powershell
$env:DEV_SUPABASE_PROJECT_REF = "qfkgjxwbshjgsxsvkpkp"
dotnet run --project .\scripts\diagnostics\HostSelect1\HostSelect1.csproj --framework net8.0
```

Expected successful output includes:

```text
TARGET CHECK: PASS
CONNECTION OPEN: PASS
SELECT 1: PASS
```

If the diagnostic reports SQLSTATE `28P01`, classify it as:

```text
HOST TLS: PASS
POSTGRES AUTH: FAIL — 28P01
SELECT 1: NOT REACHED
```

## Final summary

```text
WINDOWS: Windows 10 Home Single Language 25H2 build 26200.9457
DOTNET: .NET SDK 10.0.200; .NET 8 runtime 8.0.26
NPGSQL: 8.0.0
EF CORE PROVIDER: Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
DEV TARGET: qfkgjxwbshjgsxsvkpkp
POOLER: SESSION 5432
TCP: PASS
CODEX EXECUTION TLS: FAIL - Win32 0x8009030E / No credentials are available in the security package
HOST POWERSHELL TCP: PASS
HOST POWERSHELL TLS: PASS
CLIENT CERTIFICATE DETECTED: NO
GSS/SSPI INVOLVED: UNKNOWN; not reached and GSS option unsupported by installed provider
GSS DISABLED TEST: NOT SUPPORTED
CERTIFICATE TRUST TEST: SAME SCHANNEL CREDENTIAL ERROR
HOST POSTGRES AUTH: PASS
HOST SELECT 1: PASS
ROOT CAUSE: Codex execution/sandbox context Schannel credential acquisition limitation/error; normal Windows host connectivity is healthy
HOST-SIDE CONNECTIVITY BLOCKER: RESOLVED
CODEX POSTGRES AUTH: NOT REACHED
CODEX SELECT 1: NOT REACHED
DATABASE MUTATED: NO
MIGRATIONS EXECUTED: NO
SAFE TO RERUN TASK 37: NO — not until read-only TLS/SELECT 1 succeeds
SAFE TO PROCEED TO HOST-EXECUTED TASK 37: YES
NEXT ACTION: Human runs the controlled Development migration/startup against the verified isolated DEV target.
```

No commit or push was performed.
