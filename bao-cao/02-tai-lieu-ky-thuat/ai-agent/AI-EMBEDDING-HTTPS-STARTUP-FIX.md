# AI Embedding HTTPS Startup Fix

**Repository:** `D:\Dev\doan`
**Date:** 2026-10-17
**Scope:** certificate loader and isolated certificate/Kestrel verification only. No API process, database, migration, browser, model, or frontend was changed.

## 1. Root cause

`DevelopmentHttpsCertificateLoader.LoadPersistedPkcs12FromPem` imported the PEM pair with `X509Certificate2.CreateFromPemFile`, exported PKCS#12 bytes, then constructed the certificate with:

```csharp
X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet
```

On this Windows process user, the PKCS#12 import attempted to persist the private key in the user key store and failed with:

```text
System.Security.Cryptography.CryptographicException: Access denied.
```

A flag probe using the existing PEM pair produced:

```text
FAIL|UserKeySet|CryptographicException:Access denied.
FAIL|UserKeySet, PersistKeySet|CryptographicException:Access denied.
PASS|MachineKeySet|hasKey=True;rsa=True
PASS|MachineKeySet, PersistKeySet|hasKey=True;rsa=True
FAIL|DefaultKeySet|CryptographicException:Access denied.
```

`EphemeralKeySet` avoided the import error, but this Windows Schannel runtime could not use the resulting key for TLS server credentials. The Kestrel path therefore needs a provider-backed machine-key import.

## 2. Minimal source fix

Changed only:

[DevelopmentHttpsCertificateLoader.cs](../../../services/api/src/WebAPI/Configuration/DevelopmentHttpsCertificateLoader.cs:30)

The PKCS#12 import now uses:

```csharp
X509KeyStorageFlags.MachineKeySet |
X509KeyStorageFlags.PersistKeySet |
X509KeyStorageFlags.Exportable
```

Reason for each choice:

- `MachineKeySet`: avoids the inaccessible user key store identified by the probe.
- `PersistKeySet`: required for Schannel to retain usable server credentials in this runtime.
- `Exportable`: preserves the private key capability needed by the Kestrel/Schannel certificate object.

The PEM certificate and private key files were not changed. No certificate was generated, no certificate was installed into the Windows certificate store, and HTTPS remains enabled.

The existing method name is retained for call-site compatibility. The returned certificate is disposed by the existing application lifecycle.

## 3. Verification

### Backend build

```powershell
dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .pilotbuild
```

Result: **PASS**, 0 warnings, 0 errors.

### Isolated certificate test

Harness:

`.tools/https-certificate-verification/`

Command:

```powershell
dotnet run --project .tools/https-certificate-verification/HttpsCertificateVerification.csproj -c Release --no-restore
```

Checks performed:

- Import existing PEM pair.
- Confirm `HasPrivateKey`.
- Retrieve RSA private key.
- Sign and verify data with the private key.
- Configure and start an isolated Kestrel listener with `UseHttps(certificate)`.

Observed results:

```text
PASS|kestrel_bind|https_certificate_with_private_key
PASS|certificate_import|machine_pkcs12_private_key
```

The isolated Kestrel host started on an ephemeral loopback HTTPS port and was stopped immediately. It did not connect to the application database.

### TLS loopback note

A raw `SslStream` client/server handshake in this sandbox reported a Windows SSPI `AuthenticationException` (`No credentials are available in the security package`) even though Kestrel successfully bound the HTTPS certificate. The test is recorded as:

```text
BLOCKED|tls_loopback|Windows_SSPI_handshake_in_test_process
```

This is an environment-level SSPI handshake limitation in the auxiliary test process; it is separate from the original PKCS#12 `Access denied` import failure. No API process was started to investigate further.

## 4. Configuration requirements

The API still requires the existing Development configuration:

- `ASPNETCORE_ENVIRONMENT=Development`.
- `HTTPS_PEM_CERT_PATH` and `HTTPS_PEM_KEY_PATH` pointing to the existing PEM pair, or the existing PFX path branch.
- Existing User Secrets for database and JWT values.
- No production configuration changes.
- No migration/seed opt-in.

The repository HTTPS setup validation remains the source of truth for certificate SAN/issuer/expiry checks. It was run with `-Localhost -ValidateOnly` and reused the existing verified PEM pair.

## 5. What was not tested

- The main API process was not started, per instruction.
- No PostgreSQL/Supabase connection was opened.
- No JWT request or AI endpoint request was sent.
- No database migration or seed ran.
- No browser test ran.
- No Sentence Embedding/model/dataset/threshold code was changed.

## 6. User startup instructions

After reviewing the fix, start the backend from the user’s Development terminal using the existing HTTPS setup procedure so the PEM environment variables are present in that process. Confirm `/health` returns 200 before enabling the pilot process flag.

For a temporary pilot process override only:

```text
AI__EmbeddingPilot__Enabled=true
```

Keep the checked-in `appsettings.Development.json` value `false` for the default state. To disable/rollback, omit the override or set:

```text
AI__EmbeddingPilot__Enabled=false
```

Do not start the API from this verification harness and do not expose the private key or JWT secret.

## 7. Status

- **PASS:** root cause reproduced, minimal loader fix, backend build, existing PEM import, private-key sign/verify, isolated Kestrel HTTPS bind.
- **BLOCKED:** raw SSPI TLS loopback in the auxiliary test process.
- **NOT RUN:** main API startup, database, JWT, AI HTTP requests, browser.
- **FAIL:** none for the corrected certificate import path.

**GO:** the loader fix is ready for the user to start the Development API and verify `/health`.
**NO-GO:** do not claim live API or production readiness until the user performs that startup and HTTP verification.


## Current status / Superseded by

Historical startup fix; current: RESOLVED_FOR_OBSERVED_DEVELOPMENT_API_STARTUP. No production readiness implied.
