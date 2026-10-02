# Repository System Audit

Audit date: 2026-09-30  
Repository: `D:\Dev\doan`  
Source-of-truth rule: current executable source first, then EF model/migrations, API, services, frontend consumers, configuration, tests, and finally old documents.

This set describes what is evidenced in the repository. Labels mean:

- **IMPLEMENTED**: executable path is present and connected.
- **PARTIALLY IMPLEMENTED**: some layers exist but the end-to-end path is incomplete or inconsistent.
- **PLANNED / DESIGN ONLY**: design/documentation/TODO evidence without a verified executable path.
- **LEGACY / UNUSED**: retained code or documentation not shown to be on the current path.
- **UNKNOWN**: repository evidence is insufficient.

The audit covers backend, database/migrations, authentication/authorization, frontend applications, POS/order/kitchen, QR ordering, payment, SignalR, AI, deployment, security, tests, and old documentation. The detailed evidence is in files `01`–`17`.

Repository snapshot counts (excluding `node_modules`, build output and `dist`): 454 files; 323 C# files; 66 TypeScript/TSX files; 38 Markdown/text/document files; 168 migration-related C# files; 31 test C# files. Counts include tracked and untracked files visible in the working tree and should not be interpreted as release artifact counts.

The working tree was already dirty at audit start. Existing changes were preserved. No source, migration, configuration, or test was changed by the documentation cleanup. Phase 10 archived historical/design documents under `docs/archive/`; no document was deleted. Files that were already marked deleted before the audit remain explicitly pre-existing deletions.
