# Evidence Index

This file catalogs where evidence belongs and how it must be linked. It is an index, not proof that every listed artefact is valid or current.

## Evidence rule

Every evidence item should identify, where applicable:

- stable audit item or finding ID;
- repository snapshot/commit and whether working-tree changes were included;
- date/time and timezone;
- environment mode and non-secret target identifier;
- method: static, technical, runtime or manual;
- expected and actual result;
- sanitized path to log, screenshot, video, response or report;
- retest relationship if it replaces earlier evidence.

Never store passwords, access tokens, API keys, private certificate material or full connection strings. Mask user/customer data and QR/JWT values in screenshots, logs and exported responses.

## Existing evidence locations

| Location | Intended use | Current handling |
|---|---|---|
| [`ba-test-frontend/`](../../../../ba-test-frontend/README.md) | Manual scenario definitions and per-case observations | Keep as the manual checklist; link by case ID, do not copy cases here |
| [`evidence/`](../../../../evidence/) | Existing logs, safe diagnostics and runtime attestations | Catalog only after checking snapshot, environment and secret redaction |
| [`test-results/`](../../../../test-results/) | Existing test/report output | Treat as historical until command, version and source snapshot are verified |
| [`tests/benchmark/`](../../../../tests/benchmark/) | Benchmark/tool-boundary reports | Use only for the scope actually covered by each report |
| [`scripts/diagnostics/`](../../../../scripts/diagnostics/) | Reproducible diagnostic procedures and their possible outputs | Read procedure first; do not run apply/seed/migration operations without approval |
| Existing system audit files | Static findings and prior runtime reports | Reconcile with current source before treating as current evidence |

## Evidence entry template

```markdown
### EVID-<area>-<number> — <short description>

- Audit item/finding:
- Method: Static / Technical / Runtime / Manual
- Snapshot: commit or named working-tree snapshot
- Environment: local/test/staging; host and port only when safe
- Date/time:
- Expected:
- Actual:
- Result: PASS / FAIL / BLOCKED / NOT APPLICABLE
- Artifact: relative path or sanitized reference
- Related finding:
- Retest/replacement:
```

`PASS` is allowed only when the artifact directly proves the stated assertion. A build log proves the build command succeeded; it does not prove payment, branch isolation, database integrity or browser behavior.

## Current preparation state

No new runtime evidence was generated in this preparation step. Existing evidence files have not been revalidated against the current dirty working tree, so they remain `Chưa kiểm tra` for current-acceptance purposes until their snapshot and environment are confirmed.
