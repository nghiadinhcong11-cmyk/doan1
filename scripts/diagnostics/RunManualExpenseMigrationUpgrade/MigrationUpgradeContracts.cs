namespace RestaurantPOS.MigrationUpgrade;

public static class MigrationUpgradePolicy
{
    public const string TestProjectRef = "eorboopkwgmvainlxzkz";
    public const string TestPoolerHost = "aws-0-ap-northeast-1.pooler.supabase.com";
    public const int TestPoolerPort = 5432;
    public const string TestDatabase = "postgres";
    public const string TestUsername = "postgres.eorboopkwgmvainlxzkz";
    public const string ApprovedMigration = "20261013091926_AddManualExpenseIdempotency";
    public const string HistoricalLatestMigration = "20261005072156_RemoveLegacyTableAreaName";
    public const int HistoricalMigrationCount = 47;
    public const int AvailableMigrationCount = 48;

    public static readonly IReadOnlyList<string> HistoricalMigrations =
    [
        "20260721094114_InitialCreate",
        "20260722105813_AddBankInfoToBranch",
        "20260723095409_AddAttendanceSystem",
        "20260723100041_FixWorkScheduleBranchId",
        "20260723103527_AddBranchIdToEmployee",
        "20260723104422_AddInfoToOrder",
        "20260723105007_AddCustomerTable",
        "20260723145243_AddBranchNameToSchedule",
        "20260723150626_AddCustomerPhoneToOrder",
        "20260812051057_AddForeignKeysAndSync",
        "20260822153125_AddShiftTable",
        "20260822153908_AddReservationTable",
        "20260822155325_UpdateShiftDetails",
        "20260822162819_AddCustomerPassword",
        "20260825144406_UpdateCustomerProfile",
        "20260825162216_AddCustomerEmailToOrder",
        "20260825165006_AddProductOptionsSupport",
        "20260829062053_AddToppingTable",
        "20260829063304_UpdateToppingSchema",
        "20260829142528_AddToppingSupportToOrderDetails",
        "20260829152705_AddPromotionTable",
        "20260902072328_AddImageUrlToBranch",
        "20260902133018_AddKitchenRequests",
        "20260905050736_AddExpenseBranchAndAudit",
        "20260906040844_AddKitchenHistoryAndNotifications",
        "20260907044417_AddEmployeeTypeColumn",
        "20260907060113_AddPaymentAtToOrderCorrective",
        "20260907060600_AddMissingPayrollsAndSettings",
        "20260907063921_Corrective_FixMissingColumns",
        "20260907155909_RepairMissingDatabaseSchema",
        "20260907164814_AddSystemSettings",
        "20260908032924_AddOrderFinancialSnapshot",
        "20260908041144_AddBranchStoreProfile",
        "20260908043852_AddLoyaltyFoundation",
        "20260908045205_AddLoyaltyIdempotencyIndex",
        "20260908094237_AddRoleToEmployee",
        "20260908100201_BackfillEmployeeRoles",
        "20260910052615_AddAnalyticsIndexes",
        "20260910063419_AddBusinessInsight",
        "20260919075618_RemovePayrollAndRepairReceiptSettings",
        "20261001044428_AddInventoryManagement",
        "20261001094205_AddRestaurantTableQrToken",
        "20261001155855_FinalizeRestaurantTableQrToken",
        "20261002140936_AddToppingType",
        "20261004072155_RefineMultiBranchDomainRelationships",
        "20261005061004_EnforceMultiBranchRequiredRelationships",
        HistoricalLatestMigration
    ];

    public static string ResolveMode(IReadOnlyList<string> args) =>
        args.Count == 0 ? "--dry-run" : args.Count == 1 ? args[0] : "--invalid";

    public static bool ValidateAvailable(IEnumerable<string> migrations, out string failure)
    {
        var values = migrations.ToArray();
        var expected = HistoricalMigrations.Append(ApprovedMigration).ToArray();
        if (values.Length != AvailableMigrationCount || !values.SequenceEqual(expected, StringComparer.Ordinal))
        {
            failure = "SOURCE_MIGRATION_SEQUENCE_MISMATCH";
            return false;
        }

        failure = string.Empty;
        return true;
    }

    public static bool ValidateAppliedHistorical(IEnumerable<string> migrations, out string failure)
    {
        var values = migrations.ToArray();
        if (values.Length != HistoricalMigrationCount ||
            !values.SequenceEqual(HistoricalMigrations, StringComparer.Ordinal))
        {
            failure = "APPLIED_MIGRATION_HISTORY_MISMATCH";
            return false;
        }

        failure = string.Empty;
        return true;
    }

    public static bool ValidatePending(IEnumerable<string> migrations, out string failure)
    {
        var values = migrations.ToArray();
        if (values.Length != 1 || !string.Equals(values[0], ApprovedMigration, StringComparison.Ordinal))
        {
            failure = "UNEXPECTED_PENDING_MIGRATION";
            return false;
        }

        failure = string.Empty;
        return true;
    }

    public static bool ValidatePreUpgradeStructure(
        bool existingSchemaMatches,
        bool targetObjectsAbsent,
        bool branchPrincipalCompatible,
        out string failure)
    {
        if (!existingSchemaMatches)
        {
            failure = "PUBLIC_SCHEMA_DRIFT";
            return false;
        }

        if (!targetObjectsAbsent)
        {
            failure = "TARGET_OBJECT_ALREADY_EXISTS";
            return false;
        }

        if (!branchPrincipalCompatible)
        {
            failure = "BRANCH_FK_TARGET_INVALID";
            return false;
        }

        failure = string.Empty;
        return true;
    }
}
