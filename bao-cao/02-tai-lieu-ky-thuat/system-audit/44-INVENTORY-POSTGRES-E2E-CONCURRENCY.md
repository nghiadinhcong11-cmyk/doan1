# Task 44 — Inventory PostgreSQL E2E & Concurrency Validation

## Status

This task is prepared for a human-run normal Windows PowerShell execution. Codex has not connected to Supabase and has not mutated the DEV database.

## DEV Target and Safety Guard

The only authorized target is project `qfkgjxwbshjgsxsvkpkp`, Session Pooler `aws-0-ap-southeast-2.pooler.supabase.com:5432/postgres`, with username identity `postgres.qfkgjxwbshjgsxsvkpkp`. The runner refuses to continue unless the environment project reference, host, port, database, username identity, and SSL mode match this target. It prints no password or full connection string.

## Harness

`scripts/diagnostics/InventoryPostgresE2E/` is an opt-in console harness referencing the production ApplicationDbContext and InventoryService. It uses separate DbContext instances for operations and two independent contexts for the 7 + 7 concurrency test. It does not participate in the normal test suite, so ordinary `dotnet test` remains offline.

The harness retains records with a unique `E2E_INV_` prefix. This is intentional: confirmed documents and StockTransactions are historical/immutable and must not be deleted through production APIs. A later human-approved cleanup procedure may remove only these explicitly identified DEV records if required.

## Coverage Prepared

The runner covers receipt/issue drafts, server-side receipt total, confirmation, ledger and Expense cardinality, repeated confirmation, confirmed immutability, insufficient-stock atomicity, negative-stock rejection, draft cancellation, document idempotency, employee confirmation denial when a matching fixture exists, and PostgreSQL concurrency with two independent contexts. The service implementation uses PostgreSQL `SELECT ... FOR UPDATE` and deterministic `InventoryItemId` ordering; the runner records the concurrent 7 + 7 result.

Cross-branch role coverage is fixture-dependent. When an active second branch and matching manager fixture exist, the harness exercises the branch guard; otherwise it reports the check as skipped rather than fabricating authorization evidence. HTTP/controller-specific checks, including linked Expense edit/delete protection, should be captured by a human API smoke run if required by the environment.

## Results Before Human Run

All runtime sections are `PENDING HUMAN HOST RUN`: receipt draft/confirm, Expense, issue draft/confirm, rollback, negative stock, branch isolation, role authorization, idempotency, immutability, row locking, and final invariants.

Static baseline: API build PASS; normal backend tests PASS, 214/214. No new migration was created, and no database connection or mutation was performed by Codex.

## Initial PostgreSQL E2E Failure Diagnosis

The first human run reached `TARGET CHECK: PASS` and `CONNECTION/SELECT 1: PASS`, then stopped with a generic `DbUpdateException`. The original harness suppressed the phase, EF entity state, and PostgreSQL exception metadata, so the failing operation could not be localized.

Task 44A adds ordered `[E2E nn]` start/pass/fail markers, a read-only leftover-record count before mutation, and sanitized `DbUpdateException` diagnostics. When PostgreSQL is the inner exception, the runner reports only SQLSTATE, severity, constraint, table, column, data type, schema, and message text; it never reports connection strings, parameters, passwords, JWTs, or entity values. EF failure entries report CLR type and state only.

The harness now uses a millisecond UTC run suffix in every `E2E_INV_` item/document name. The human rerun will report whether earlier attempts left records in InventoryItems, StockReceipts, StockIssues, StockTransactions, or Expenses. No cleanup is performed by this diagnostic task.

Current diagnosis: `UNKNOWN` until one instrumented normal-host rerun returns the first failed phase and PostgreSQL metadata. No production Inventory code, schema, migration, or database was changed.

## Receipt Expense PaymentMethod Runtime Defect

The authorized DEV rerun reached receipt confirmation and failed with PostgreSQL SQLSTATE `23502`: `Expenses.PaymentMethod` is `NOT NULL`. EF reported the expected atomic unit (`BranchInventory` modified, `StockTransaction` added, `Expense` added, and `StockReceipt` modified) before the failed `SaveChanges`; the service uses one database transaction and commits only after `SaveChanges`, so the failed confirmation is statically atomic and did not commit those changes. The human report retained two `InventoryItems` and one Draft `StockReceipt`; no transaction or Expense was left by that attempt.

Source audit found the existing Expense schema was created with non-null `PaymentMethod`. Existing order and frontend contracts use the canonical values `Tiền mặt` and `Chuyển khoản`; the normal Expense UI defaults to `Tiền mặt`, while the CLR Expense property was incorrectly nullable and Inventory receipt confirmation omitted it. The fix uses a small shared `ExpensePaymentMethods` catalog, makes the CLR property non-null with the existing cash default, and sets Inventory-created receipt Expenses explicitly to `Tiền mặt`. Normal Expense controller creation/update also defaults omitted input to the same existing value, preserving the database contract without a schema change.

Inventory regression coverage now verifies the linked Expense has a non-null canonical PaymentMethod, category `Nhập hàng`, the server-calculated amount, and the receipt link. The E2E harness performs the same checks after confirmation. No migration was created and no database schema was modified. Human rerun remains required.

## Full Expense Contract Reconciliation

The Expense contract was audited against `20260721094114_InitialCreate`, `20260905050736_AddExpenseBranchAndAudit`, the applied Inventory migration, the current model snapshot, `Expense`, `ApplicationDbContext`, `ExpenseController`, DTOs, frontend defaults, tests, and InventoryService.

| Column | CLR | CLR nullable | EF required | PostgreSQL | DB nullable | DB default | Inventory confirmation |
|---|---|---:|---:|---|---:|---|---|
| Id | Guid | No | Yes | uuid | No | None | Generated |
| BranchId | Guid | No | Yes | uuid | No | None | Receipt branch |
| Description | string | No | Yes | text | No | None | Deterministic supplier description |
| Amount | decimal | No | Yes | numeric | No | None | Server-calculated receipt total |
| ExpenseDate | DateTime | No | Yes | timestamp with time zone | No | None | PurchaseDate or confirmation time |
| Category | string | No | Yes | text | No | None | `Nhập hàng` |
| PaymentMethod | string | No | Yes | text | No | None | `Tiền mặt` |
| Note | string | No | Yes | text | No | None | Receipt note or `Phiếu nhập kho <id>` |
| CreatedBy | Guid? | Yes | No | uuid | Yes | None | Authenticated user id |
| StockReceiptId | Guid? | Yes | No | uuid | Yes | None | Receipt id |
| CreatedAt | DateTime | No | Yes | timestamp with time zone | No | None | Confirmation time |
| UpdatedAt | DateTime? | Yes | No | timestamp with time zone | Yes | None | Not applicable to new linked Expense |

The initial migration created `PaymentMethod` and `Note` as NOT NULL. The Expense audit migration made `BranchId` NOT NULL after backfill and kept `CreatedBy`/`UpdatedAt` nullable. The Inventory migration added nullable `StockReceiptId` with a unique filtered index and restricted foreign key. No Expense database default supplies missing values.

The prior runtime failure exposed `PaymentMethod`; the next runtime failure exposed `Note`. The source mismatch was that both CLR properties were optional while the database was not, and InventoryService supplied neither value reliably. The CLR model now uses non-null defaults for both fields, EF configuration explicitly requires both, and receipt confirmation supplies every required column. Optional `StockReceipt.Note` is never copied as null: it becomes the receipt note when present or a deterministic `Phiếu nhập kho <receipt id>` fallback.

The current snapshot had represented `PaymentMethod` and `Note` as optional before this reconciliation, while migration/runtime evidence says NOT NULL. The snapshot was not manually edited and no migration was generated in this task; this known model/snapshot history discrepancy must be handled through the repository's normal future migration-review workflow, without changing the already-correct DEV schema.

Required-field coverage for Inventory-created Expenses: **9/9 non-null Expense columns supplied** (`Id`, `BranchId`, `Description`, `Amount`, `ExpenseDate`, `Category`, `PaymentMethod`, `Note`, `CreatedAt`). Nullable audit/link fields are also populated where applicable (`CreatedBy`, `StockReceiptId`). The single transaction still covers BranchInventory, StockTransaction, Expense, and StockReceipt confirmation; the two failed DEV attempts left 0 transactions and 0 Expenses, supporting rollback. Full runtime success remains pending.

## Human PowerShell Commands

Run from `D:\Dev\doan` in normal Windows PowerShell:

```powershell
$env:DEV_SUPABASE_PROJECT_REF = "qfkgjxwbshjgsxsvkpkp"
dotnet run --project .\scripts\diagnostics\HostSelect1\HostSelect1.csproj --framework net8.0
dotnet run --project .\scripts\diagnostics\InventoryPostgresE2E\InventoryPostgresE2E.csproj --framework net8.0
dotnet run --project .\scripts\diagnostics\PostMigrationInventoryValidation\PostMigrationInventoryValidation.csproj --framework net8.0
```

Return the complete non-secret output, including the E2E prefix, every PASS/FAIL/SKIPPED line, concurrency result/final stock, and read-only post-migration validation output. Do not paste credentials.

## Phase 3 Readiness

Until the human harness completes successfully, Phase 2 remains `PARTIAL` and the frontend should not be declared safe based on runtime PostgreSQL evidence. After a successful host run, update this report with actual results and reassess Phase 3 readiness.
