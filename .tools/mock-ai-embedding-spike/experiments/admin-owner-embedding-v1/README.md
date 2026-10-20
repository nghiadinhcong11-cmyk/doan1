# Admin owner embedding v1

Offline research experiment only. It is not connected to `AiOrchestrator`, `AiToolRegistry`, an API endpoint, or a database.

The reference catalog is frozen before calibration. `calibration.json` is used only for score/margin selection. `holdout.json` is evaluated after the configuration is selected and must not be copied into the reference catalog or used for tuning.

Run from `.tools/mock-ai-embedding-spike` after building the existing spike project:

```powershell
dotnet run -c Release --no-restore -- --admin-embedding-eval --artifact-root artifacts --admin-root experiments/admin-owner-embedding-v1 --output artifacts/admin-owner-embedding-v1-result.json
```

The runner performs semantic classification, explicit policy decisions for OOD/negation/multi-intent labels, and deterministic period/status/limit slot checks. It never executes a business tool.
