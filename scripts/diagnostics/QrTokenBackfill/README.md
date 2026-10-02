# QrTokenBackfill

Controlled maintenance utility for the one-time `RestaurantTable.QrToken` backfill.

It is dry-run by default. It will write only when both conditions hold:

1. `DEV_SUPABASE_PROJECT_REF=qfkgjxwbshjgsxsvkpkp` is explicitly supplied and matches the configured connection identity.
2. The command includes `--execute`.

The utility verifies Migration A, the nullable `QrToken varchar(43)` column, and the unique filtered index before it queries rows. It updates only rows whose token is still `NULL`, using `QrTokenGenerator.Generate()` inside one transaction. It never logs token values and preserves non-null tokens.

Example for an explicitly authorized isolated DEV operation:

```powershell
$env:DEV_SUPABASE_PROJECT_REF='qfkgjxwbshjgsxsvkpkp'
dotnet run --project scripts/diagnostics/QrTokenBackfill/QrTokenBackfill.csproj -- --execute
```

Production or any other target requires separate authorization and must not reuse this DEV command.
