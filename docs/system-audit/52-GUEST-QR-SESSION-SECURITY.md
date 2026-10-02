# Task 47H.1 — Guest QR Table Session Audit

## Verdict

The requested QrToken bootstrap cannot be implemented from the current repository without a schema decision. Contrary to the task premise, `RestaurantTable` contains `QrCodeUrl` but no `QrToken` property. `ApplicationDbContext`, all current migrations, controllers, services, frontend code, and tests also contain no `QrToken` mapping, generation, index, or uniqueness constraint.

## Current flow

`QR data / ?tableId=<GUID> -> GET /api/Table/{tableId} -> Customer Web sets table/branch display state -> POST /api/Order with client-controlled table and branch values.`

`QRScan.tsx` accepts either a `tableId` URL parameter or a raw GUID. `DigitalMenu.tsx` reads the same query parameter and looks up the table by ID. The current guest token endpoint is `POST /api/Auth/customer-token`; it resolves only a customer phone and creates a generic `customerSessionType=guest` JWT without table or branch claims.

## Security consequence

There is no persisted server-known bootstrap secret that can establish the required chain:

`QrToken -> Table -> Branch -> Guest JWT`.

Treating `QrCodeUrl` or a table GUID as a token would not meet the requested authority boundary. It has no evidenced entropy, uniqueness, revocation, or lookup guarantee. Adding JWT table/branch claims without a trusted server-side QR token would merely sign client-selected context.

## Required decision before implementation

Approve a narrowly scoped schema/API design that introduces a server-generated, high-entropy, unique table QR token and its lifecycle. The design must define:

1. token storage and unique database enforcement;
2. secure token generation at table creation and any regeneration/revocation path;
3. a `QrToken` bootstrap endpoint returning a guest token with signed table and branch claims;
4. guest-order authorization derived from those claims; and
5. migration review/application separately from this task.

No migration, source behavior change, database connection, or database mutation was performed in Task 47H.1.

## Separate policy boundary

Phone-only registered-customer identity remains deferred to Task 47H.2 and was not changed.

## Task 47H.1A — Secure token foundation migration review

The schema decision to add a persisted `RestaurantTable.QrToken` is approved, but migration generation was deliberately stopped before source or schema changes.

The required final migration must add a non-null, globally unique, opaque token and safely assign a distinct high-entropy token to every existing table. The repository contains no established PostgreSQL cryptographic token primitive or extension (`pgcrypto`, `gen_random_bytes`, `gen_random_uuid`, or `uuid_generate_*`). Using PostgreSQL `random()`/`md5`, a table identifier, or one shared default would not meet the QR possession-token security contract.

Additionally, the current EF model snapshot and `ApplicationDbContext` already contain unrelated pending Inventory, Payroll-removal, and analytics changes. Creating a new migration now would fold that unrelated drift into the QR migration, contrary to the requirement that it touch only the table token foundation.

No `QrToken` model field, migration, index, generator, database connection, or database mutation was created in Task 47H.1A. A follow-up must first establish the canonical migration baseline and explicitly approve a verified secure PostgreSQL backfill mechanism (including any required extension and its operational availability). Once those conditions are met, the intended sequence remains: nullable column, secure per-row backfill, `NOT NULL`, and unique index; application table creation must generate 32 cryptographically random bytes encoded Base64Url.

Task 47H.1A.1 narrowed the EF issue to stale snapshot metadata for the already-`NOT NULL` `Expenses.PaymentMethod` and `Expenses.Note` contract; Inventory, payroll removal, and analytics do not produce pending EF operations. Task 47H.1A.2 repaired those two snapshot entries and an isolated EF diagnostic migration was empty. See `53-EF-MIGRATION-BASELINE-RECONCILIATION.md`. The EF baseline is reconciled; the QrToken foundation may now proceed in its own migration task.

## Task 47H.1A.3 — QrToken Migration A

Source now includes nullable `RestaurantTable.QrToken` through migration `20261001094205_AddRestaurantTableQrToken`. The migration adds only `Tables.QrToken` as `varchar(43)` and a unique filtered index for non-null values. It is intentionally nullable because existing rows have not yet received securely generated tokens. The migration has not been applied and no database was contacted.

The current source migration inventory contains 45 migration classes after this addition, while earlier audit documentation recorded 41 before it. This pre-existing count discrepancy must be reconciled in human migration review before Migration A is applied; no pre-existing migration was altered.

New tables receive a server-generated 256-bit token from `RandomNumberGenerator`, encoded Base64Url without padding (43 characters). The creation service overwrites any client-supplied value. The public table API now returns an explicit response DTO that preserves prior table fields and `QrCodeUrl` while excluding `QrToken`.

The legacy `QrCodeUrl` and Customer Web `tableId` QR flow remain active and unchanged. Existing rows must be backfilled by the separately approved controlled .NET process before Migration B can enforce `NOT NULL`.

## Task 47H.1A.4 — Migration A applied to isolated DEV

At **2026-10-01 21:57:17 +07:00**, the configured isolated DEV Supabase project `qfkgjxwbshjgsxsvkpkp` was identity-verified and Migration A was explicitly applied. The pre-apply history contained all 41 canonical predecessor migrations, no unknown IDs, and no QrToken column/index. The resulting history contains 42 migrations with `20261001094205_AddRestaurantTableQrToken` latest exactly once.

Runtime schema verification found nullable `Tables.QrToken varchar(43)` with no default and unique filtered index `IX_Tables_QrToken` using `"QrToken" IS NOT NULL`. All 7 existing table rows have `QrToken = NULL`; no token backfill, QR bootstrap, Customer Web change, or Guest JWT change was performed. This is schema-foundation evidence only and does **not** complete Guest QR security.

## Task 47H.1A.5 — controlled DEV backfill evidence

At **2026-10-01 22:21:22 +07:00**, the retained `scripts/diagnostics/QrTokenBackfill` utility backfilled only null-token rows on verified isolated DEV project `qfkgjxwbshjgsxsvkpkp`. It defaults to dry run and required both the explicit DEV-project environment guard and `--execute`; it reused `QrTokenGenerator.Generate()` and performed conditional `QrToken IS NULL` updates in one transaction.

The initial dry run reported 7 total tables, 7 null tokens, 0 non-null tokens, and 0 writes. Execution populated 7 rows. Aggregate verification reported 7 total, 0 null, 7 non-null, 7 distinct, 0 invalid lengths, and 0 invalid Base64Url formats. A second dry run and explicit execute wrote zero rows, proving idempotency. No actual tokens were logged. Migration B is not created, and this still does **not** implement QR bootstrap, Guest JWT table/branch binding, or complete Guest QR security.

## Task 47H.1A.6 — final NOT NULL contract, unapplied

Migration B `20261001155855_FinalizeRestaurantTableQrToken` is created but **not applied**. It makes `Tables.QrToken varchar(43)` required without adding a default or generating replacement values. It drops Migration A's filtered unique index, applies the NOT NULL alteration, and recreates `IX_Tables_QrToken` as an ordinary unique index; its Down restores the nullable filtered-index state. The only prerequisite is the separately verified secure backfill (`QrToken NULL count = 0`) for the target environment.

The domain/EF model now requires `RestaurantTable.QrToken`; normal table creation remains server-generated through `QrTokenGenerator.Generate()`, and public table responses continue to omit it. Migration B contains no guest bootstrap, JWT binding, or public API change. It does not complete Guest QR security.

## Task 47H.1A.7 — QrToken persistence foundation applied on isolated DEV

At **2026-10-01 23:16:05 +07:00**, Migration B `20261001155855_FinalizeRestaurantTableQrToken` was explicitly applied to the identity-verified isolated DEV Supabase project `qfkgjxwbshjgsxsvkpkp`. The DEV history now contains all 43 canonical migrations with Migration B latest exactly once and no unknown IDs.

Post-apply PostgreSQL metadata confirms `Tables.QrToken` is `varchar(43) NOT NULL`, has no default, and is protected by ordinary unfiltered unique index `IX_Tables_QrToken`. Aggregate validation found 7 total rows, 0 null tokens, 7 non-null and distinct tokens, and no invalid length/Base64Url values. A private before/after mapping fingerprint matched, confirming the migration did not regenerate or change token values; actual tokens were not logged.

This completes only the persisted QrToken foundation. It does **not** complete Guest QR security: trusted `QrToken -> Table -> Branch -> Guest JWT` bootstrap, guest-order enforcement, Customer Web migration, and runtime validation remain future work.

## Task 47H.1B — secure bootstrap and trusted Guest QR session

New physical QR URLs use `?qr=<opaque QrToken>`. `POST /api/guest/bootstrap` accepts only that 43-character Base64Url token, resolves the active persisted table and its branch server-side, and returns only a guest JWT plus public table/branch display context. Invalid, unknown, inactive, and ambiguous legacy table-name mappings return the same safe invalid-QR response without identifiers.

The JWT remains role `customer` for compatibility but is authoritatively distinguished by `customerSessionType=guest` and signed `tableId` / `branchId` claims. `GuestSessionContext` parses those claims centrally and fails closed. Guest order writes overwrite body table/branch values from the trusted context; guest list/detail reads require the order to match that table and branch. Guest AI remains denied by the existing guest-session policy.

Anonymous table-by-GUID detail access is now authenticated-only and no longer establishes guest authority. The legacy phone token endpoint no longer creates unbound guest tokens. Customer Web uses a separate `guestToken` plus `sessionStorage` guest-session marker, preserving `customerToken` for registered customers. Authorized table managers obtain a token only through `GET /api/Table/{id}/qr-token` for QR generation; public table responses still omit it. Existing persisted `QrCodeUrl` values were deliberately not mass-updated.

Automated source coverage verifies token bootstrap, claim derivation, malformed/missing claims, write injection resistance, and cross-table/branch read denial. Browser/device QR scan and end-to-end validation remain required. Registered-customer phone identity verification remains explicitly deferred to Task 47H.2.
