# Findings Register

This is the single register for audit findings. Other checklists should reference an existing ID instead of copying the same issue. A documentation discrepancy or missing evidence can be a finding; it is not automatically an application defect.

## Status and severity

`Tráº¡ng thĂ¡i xá»­ lĂ½` and `Má»©c Ä‘á»™ nghiĂªm trá»ng` are separate:

- Tráº¡ng thĂ¡i: `Má»Ÿ`, `Äang xá»­ lĂ½`, `Chá» kiá»ƒm tra láº¡i`, `ÄĂ£ Ä‘Ă³ng`, `ÄÆ°á»£c cháº¥p nháº­n cĂ³ Ä‘iá»u kiá»‡n`.
- Severity: `Critical`, `High`, `Medium`, `Low`, `Observation`.
- Priority: `P0` (must resolve before acceptance), `P1` (should resolve), `P2` (can defer). Priority is scheduling, not severity.

## Finding template

| Field | Required content |
|---|---|
| ID | Stable identifier, e.g. `BASE-001` |
| Finding | Concise statement of the observed issue or uncertainty |
| Evidence | Exact path, command/result, version and environment; no secrets |
| Impact | What acceptance, security, data integrity or reproducibility could be affected |
| Severity | Critical/High/Medium/Low/Observation |
| Priority | P0/P1/P2 |
| Processing status | Má»Ÿ/Äang xá»­ lĂ½/Chá» kiá»ƒm tra láº¡i/ÄĂ£ Ä‘Ă³ng/Cháº¥p nháº­n cĂ³ Ä‘iá»u kiá»‡n |
| Remediation | Proposed owner/action, without changing code in this register |
| Retest condition | Concrete condition and method required to close |
| Retest result | Date, snapshot, method and evidence link |

## Findings

### BASE-001 â€” Working tree is not a reproducible clean snapshot

| Field | Value |
|---|---|
| Evidence | `git status --porcelain=v1 -uall` at the survey time recorded in [`REPOSITORY-SNAPSHOT.md`](../01-baseline/REPOSITORY-SNAPSHOT.md): 137 modified, 149 deleted, 19,474 untracked entries; untracked output includes dependency/build artefacts |
| Impact | Results cannot be reproduced from HEAD alone; source, generated files and pre-existing edits may be mixed |
| Severity | Medium |
| Priority | P0 |
| Processing status | Má»Ÿ |
| Remediation | Freeze a named review snapshot or clearly separate source changes before acceptance evidence is finalized |
| Retest condition | Re-run status on the agreed snapshot and record the exact inclusion/exclusion rules for generated files |

### DOC-001 â€” Migration count in current source differs from an existing state document

| Field | Value |
|---|---|
| Evidence | Static count under `services/api/src/Infrastructure/Persistence/Migrations/`: 49 non-designer migration files; latest is `20261014120000_AddNotificationReadState.cs`. [`CODEX-CURRENT-STATE.md`](../CODEX-CURRENT-STATE.md) records 43 and an older latest migration |
| Impact | Reviewers may use an outdated migration baseline or mistake documentation drift for database failure |
| Severity | Medium |
| Priority | P0 |
| Processing status | Má»Ÿ |
| Remediation | Reconcile the documentation against the agreed source snapshot and separately record actual applied-database history |
| Retest condition | On an authorized isolated database, compare EF-discovered migrations, migration history and model snapshot; record results without changing the database in an unapproved environment |

This finding records a verified documentation/source-count discrepancy only. It does not conclude that the database is broken, that all 49 migrations are applied, or that any migration should be removed.

### PAY-001 â€” Historical migration chain needs a cleanup decision

| Field | Value |
|---|---|
| Evidence | Static inspection of `ApplicationDbContextModelSnapshot.cs` found `Employee` properties `BasicSalary` and `EmployeeType`, but no `Payroll`, `PayrollAdjustment` or `PayrollSettings` entity block, payroll table mapping, payroll foreign key or payroll relationship. The current `ApplicationDbContext` exposes `DbSet<Employee>` and configures the employee-to-branch relationship. Historical payroll identifiers remain in earlier/later migration source and designer files, including `20260907060600`, `20260907155909` and `20260919075618`. `EmployeeType` and `BasicSalary` are intentionally retained as employee profile fields and are outside this finding. |
| Impact | The current snapshot does not itself preserve payroll entity/table metadata, but the historical migration chain still contains payroll operations and designer metadata. Blind deletion, reordering or snapshot rewriting could break replay/upgrade assumptions or unrelated module changes. |
| Severity | High |
| Priority | P0 |
| Processing status | Má»Ÿ |
| Remediation | Keep the current employee profile fields and current snapshot mappings. Separately review the complete migration chain and choose either preserving the historical chain or adopting a separately approved baseline strategy before any migration-source rewrite. |
| Retest condition | Static chain review followed by an approved isolated migration/model comparison; do not edit `__EFMigrationsHistory` or run against the configured database. |
| Retest result | ChÆ°a kiá»ƒm tra |

## Method tracking for open findings

| Finding | PhÆ°Æ¡ng phĂ¡p | Tráº¡ng thĂ¡i thá»±c hiá»‡n | Káº¿t luáº­n | Minh chá»©ng |
|---|---|---|---|---|
| BASE-001 | Static Git inspection | ÄĂ£ xĂ¡c minh tÄ©nh | ChÆ°a káº¿t luáº­n vá» reproducibility | `REPOSITORY-SNAPSHOT.md` |
| BASE-001 | Technical snapshot freeze | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n | ChÆ°a cĂ³ agreed clean/frozen snapshot |
| DOC-001 | Static source/document comparison | ÄĂ£ xĂ¡c minh tÄ©nh | ChĂªnh lá»‡ch tĂ i liá»‡u Ä‘Ă£ xĂ¡c minh | `SOURCE-OF-TRUTH.md`, current migration directory, `CODEX-CURRENT-STATE.md` |
| DOC-001 | Database migration-history comparison | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n vá» applied schema | ChÆ°a káº¿t ná»‘i database theo giá»›i háº¡n lÆ°á»£t nĂ y |

| PAY-001 | Static source/migration-chain inspection | ÄĂ£ xĂ¡c minh tÄ©nh | Employee profile fields are retained; historical migration metadata remains under separate review | canonical migration/designer files; `ApplicationDbContextModelSnapshot.cs` |
| PAY-001 | Isolated migration/database comparison | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n | ChÆ°a cĂ³ mĂ´i trÆ°á»ng Ä‘Æ°á»£c phĂª duyá»‡t |

No item in this register is marked `PASS`.


### PAY-001 verification addendum — snapshot comparison

Static comparison completed: `ApplicationDbContextModelSnapshot.cs` contains `EmployeeType` and `BasicSalary` under `Employee`, and contains no `Payroll`, `PayrollAdjustment` or `PayrollSettings` entity blocks, payroll table mappings, foreign keys or relationships. `ApplicationDbContext.cs` contains `DbSet<Employee>` and the employee-to-branch configuration; current Domain/Application/WebAPI source has no active payroll model. Historical migration source/designer references remain separately. This addendum is static evidence only; no migration or database was changed.
### COV-001 — Guest policy for direct customer routes is not established by source

| Field | Value |
|---|---|
| Evidence | `apps/customer-web/src/App.tsx` declares `/reservation` and `/profile` in the shared Customer Web shell. The shell renders `CustomerLogin` when neither customer nor guest/QR mode is active; in guest/QR mode it renders both routes. No separate guest route guard is visible in the inspected source. |
| Impact | A route declaration alone does not establish whether a QR guest may view or mutate reservation/profile data. Manual testing could otherwise mistake rendered UI for an authorization decision. |
| Severity | Medium |
| Priority | P1 |
| Processing status | Mở |
| Remediation | Decide the intended guest policy, then verify API authorization and direct-route behavior in an isolated runtime environment. |
| Retest condition | Test unauthenticated, guest QR and authenticated customer sessions against both routes; record redirect/API status and data scope without using production data. |
| Retest result | Chưa kiểm tra |

