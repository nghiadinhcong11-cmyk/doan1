# Historical non-EF migration-like files

These three files were handwritten classes that derived from EF Core's `Migration`,
but were never discoverable by `ApplicationDbContext`: they had no generated
designer partial supplying `[DbContext]` and `[Migration]` metadata.

They are preserved here for auditability and must not be restored to
`services/api/src/Infrastructure/Persistence/Migrations/` or compiled into the
application assembly. The discoverable migration chain independently covers their
required final effects: `RepairMissingDatabaseSchema` supplies the historical
repair operations, while `RemovePayrollAndRepairReceiptSettings` removes the
Payroll subsystem and creates the final `ReceiptSettings` schema.

Archiving them does not alter EF's recognized migration list. It does not reconcile
an existing database; any existing DEV database must have its migration history and
schema inspected under separately authorized work before migrations are applied.
