# Task 47H.1R — Topping schema drift repair

## Finding

`Domain.Entities.Topping` and the EF model contained nullable `ToppingType`, and the model snapshot already included `Toppings.ToppingType`. The canonical migration chain did not: `20260829062053_AddToppingTable` created the table without it, and `20260829063304_UpdateToppingSchema` added only the other later columns. This was a missing canonical migration, not a model-pending change detectable by EF because the snapshot was already ahead.

Before repair, the authorized isolated DEV database returned HTTP 500 from `GET /api/Topping` because PostgreSQL lacked the selected column. The table had zero rows, so no data transformation was required.

## Repair

Migration `20261002140936_AddToppingType` was generated and reviewed as a narrow migration. Its only `Up` operation is:

```sql
ALTER TABLE "Toppings" ADD "ToppingType" text NULL;
```

Its `Down` operation drops only that column. It was explicitly applied to the isolated DEV project `qfkgjxwbshjgsxsvkpkp` using EF Core. Automatic startup migrations remain disabled.

## Verification

- API build: passed with 0 warnings and 0 errors.
- EF pending-model check after the migration: no changes since the last migration.
- Customer Web production build: passed.
- Backend tests: 267 passed, 0 failed, 0 skipped.
- HTTPS API logs after apply show successful `Toppings` queries; the Customer Web menu loaded 7 products.
- Browser Guest QR order flow completed successfully for the original branch/table and for a temporary second branch/table.
- Temporary cross-branch validation data was removed by the harness; no secrets, JWTs, QR tokens, or credentials were recorded.

## Scope and compatibility

No existing migration was edited or deleted. No unrelated model changes were included. The column is nullable to match the existing C# contract and preserve compatibility with existing data. This repair does not change topping pricing or order financial ownership; authoritative pricing remains in the backend order service.
