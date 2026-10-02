using Microsoft.Extensions.Configuration;
using Npgsql;

const string authorizedProjectRef = "qfkgjxwbshjgsxsvkpkp";
const string authorizedHost = "aws-0-ap-southeast-2.pooler.supabase.com";
const string authorizedUsernameFragment = "postgres.qfkgjxwbshjgsxsvkpkp";
const string targetMigration = "20261001044428_AddInventoryManagement";

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var configuredProjectRef = Environment.GetEnvironmentVariable("DEV_SUPABASE_PROJECT_REF");
if (!string.Equals(configuredProjectRef, authorizedProjectRef, StringComparison.Ordinal))
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("DEV_SUPABASE_PROJECT_REF does not match the authorized DEV project reference.");
    return 2;
}

var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? configuration["SUPABASE_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("ConnectionStrings:DefaultConnection is missing.");
    return 2;
}

NpgsqlConnectionStringBuilder connectionString;
try
{
    connectionString = NormalizeConnectionString(rawConnectionString);
}
catch (Exception ex) when (ex is ArgumentException or UriFormatException)
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine($"Connection configuration is invalid: {ex.Message}");
    return 2;
}

var targetIsAuthorized =
    string.Equals(connectionString.Host, authorizedHost, StringComparison.OrdinalIgnoreCase) &&
    connectionString.Port == 5432 &&
    string.Equals(connectionString.Database, "postgres", StringComparison.Ordinal) &&
    (connectionString.Username ?? string.Empty).Contains(authorizedUsernameFragment, StringComparison.OrdinalIgnoreCase) &&
    connectionString.SslMode != SslMode.Disable;

Console.WriteLine($"TARGET PROJECT REF: {authorizedProjectRef}");
Console.WriteLine($"HOST: {connectionString.Host}");
Console.WriteLine($"PORT: {connectionString.Port}");
Console.WriteLine($"DATABASE: {connectionString.Database}");

if (!targetIsAuthorized)
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("Configured target does not match the authorized DEV host, port, database, username, and SSL requirements.");
    return 2;
}

Console.WriteLine("TARGET CHECK: PASS");
Console.WriteLine("VALIDATION MODE: READ-ONLY METADATA");

await using var connection = new NpgsqlConnection(connectionString.ConnectionString);
try
{
    await connection.OpenAsync();
    Console.WriteLine("CONNECTION OPEN: PASS");

    var selectOne = await ScalarAsync<int>(connection, "SELECT 1");
    Console.WriteLine($"SELECT 1: {(selectOne == 1 ? "PASS" : "FAIL")}");

    var validation = true;
    validation &= await ValidateMigrationHistoryAsync(connection);
    validation &= await ValidateTablesAsync(connection);
    await ReportCheckConstraintsAsync(connection);
    validation &= await ValidateExpenseAsync(connection);
    validation &= await ValidateBranchInventoryAsync(connection);
    validation &= await ValidateInventoryItemAsync(connection);
    validation &= await ValidateDocumentsAsync(connection);
    validation &= await ValidateEmployeePreservationAsync(connection);
    validation &= await ValidatePayrollAbsenceAsync(connection);

    Console.WriteLine($"DATABASE MUTATION: NONE");
    Console.WriteLine($"POST-MIGRATION VALIDATION: {(validation ? "PASS" : "FAIL")}");
    return validation ? 0 : 1;
}
catch (PostgresException ex)
{
    Console.WriteLine("POST-MIGRATION VALIDATION: FAIL");
    Console.WriteLine($"POSTGRES SQLSTATE: {ex.SqlState}");
    Console.WriteLine("SANITIZED ERROR: PostgreSQL read-only validation query failed.");
    return 1;
}
catch (NpgsqlException ex)
{
    Console.WriteLine("POST-MIGRATION VALIDATION: FAIL");
    Console.WriteLine($"NPGSQL ERROR: {ex.GetType().Name}");
    return 1;
}

static async Task<bool> ValidateMigrationHistoryAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- MIGRATION HISTORY ---");
    var targetCount = await ScalarAsync<long>(connection, """
        SELECT COUNT(*) FROM public."__EFMigrationsHistory"
        WHERE "MigrationId" = $1
        """, targetMigration);
    Console.WriteLine($"{targetMigration}: {(targetCount == 1 ? "PRESENT ONCE" : $"COUNT {targetCount}")}");

    var preceding = new[]
    {
        "20260910052615_AddAnalyticsIndexes",
        "20260910063419_AddBusinessInsight",
        "20260919075618_RemovePayrollAndRepairReceiptSettings"
    };

    var allPresent = true;
    foreach (var migration in preceding)
    {
        var present = await ScalarAsync<bool>(connection, """
            SELECT EXISTS (
                SELECT 1 FROM public."__EFMigrationsHistory" WHERE "MigrationId" = $1
            )
            """, migration);
        allPresent &= present;
        Console.WriteLine($"{migration}: {(present ? "PRESENT" : "MISSING")}");
    }

    return targetCount == 1 && allPresent;
}

static async Task<bool> ValidateTablesAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- INVENTORY TABLES ---");
    var tables = new[]
    {
        "InventoryItems", "BranchInventories", "StockReceipts", "StockReceiptItems",
        "StockIssues", "StockIssueItems", "StockTransactions"
    };

    var present = 0;
    foreach (var table in tables)
    {
        var exists = await TableExistsAsync(connection, table);
        present += exists ? 1 : 0;
        Console.WriteLine($"TABLE {table}: {(exists ? "EXISTS" : "MISSING")}");
    }

    Console.WriteLine($"INVENTORY TABLES: {present}/{tables.Length}");
    return present == tables.Length;
}

static async Task<bool> ValidateExpenseAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- EXPENSE LINK ---");
    var column = await ReadColumnAsync(connection, "Expenses", "StockReceiptId");
    var fk = await ForeignKeyExistsAsync(connection, "Expenses", "FK_Expenses_StockReceipts_StockReceiptId");
    var uniqueIndex = await UniqueIndexExistsAsync(connection, "Expenses", "IX_Expenses_StockReceiptId");
    var pass = column is not null && column.Value.UdtName == "uuid" && column.Value.IsNullable && fk && uniqueIndex;
    Console.WriteLine($"Expenses.StockReceiptId: {(column is null ? "MISSING" : $"{column.Value.UdtName}, nullable={column.Value.IsNullable}")}");
    Console.WriteLine($"Expense FK: {(fk ? "PASS" : "FAIL")}");
    Console.WriteLine($"Expense unique link: {(uniqueIndex ? "PASS" : "FAIL")}");
    return pass;
}

static async Task<bool> ValidateBranchInventoryAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- BRANCH INVENTORY ---");
    var expectedColumns = new[] { "Id", "BranchId", "InventoryItemId", "CurrentQuantity", "MinimumStock", "UpdatedAtUtc" };
    var columnsPresent = true;
    foreach (var column in expectedColumns)
        columnsPresent &= await ColumnExistsAsync(connection, "BranchInventories", column);

    var current = await ReadColumnAsync(connection, "BranchInventories", "CurrentQuantity");
    var minimum = await ReadColumnAsync(connection, "BranchInventories", "MinimumStock");
    var unique = await UniqueIndexExistsAsync(connection, "BranchInventories", "IX_BranchInventories_BranchId_InventoryItemId");
    var branchFk = await ForeignKeyExistsAsync(connection, "BranchInventories", "FK_BranchInventories_Branches_BranchId");
    var itemFk = await ForeignKeyExistsAsync(connection, "BranchInventories", "FK_BranchInventories_InventoryItems_InventoryItemId");
    var currentCheck = await CheckConstraintExistsAsync(connection, "BranchInventories", "CK_BranchInventories_CurrentQuantity_NonNegative");
    var minimumCheck = await CheckConstraintExistsAsync(connection, "BranchInventories", "CK_BranchInventories_MinimumStock_NonNegative");

    var precision = current is not null && minimum is not null &&
        current.Value.DataType == "numeric" && current.Value.NumericPrecision == 18 && current.Value.NumericScale == 3 &&
        minimum.Value.DataType == "numeric" && minimum.Value.NumericPrecision == 18 && minimum.Value.NumericScale == 3;
    Console.WriteLine($"BranchInventory columns: {(columnsPresent ? "PASS" : "FAIL")}");
    Console.WriteLine($"Quantity precision: {(precision ? "PASS" : "FAIL")}");
    Console.WriteLine($"BranchInventory unique: {(unique ? "PASS" : "FAIL")}");
    Console.WriteLine($"Branch FK: {(branchFk ? "PASS" : "FAIL")}; InventoryItem FK: {(itemFk ? "PASS" : "FAIL")}");
    Console.WriteLine($"CurrentQuantity check: {(currentCheck ? "PASS" : "FAIL")}");
    Console.WriteLine($"MinimumStock check: {(minimumCheck ? "PASS" : "FAIL")}");
    return columnsPresent && precision && unique && branchFk && itemFk && currentCheck && minimumCheck;
}

static async Task<bool> ValidateInventoryItemAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- INVENTORY ITEM ---");
    var unitCode = await ColumnExistsAsync(connection, "InventoryItems", "UnitCode");
    var forbidden = new[] { "MinimumStock", "BranchId", "ProductId", "RestaurantId" };
    var forbiddenPresent = false;
    foreach (var column in forbidden)
        forbiddenPresent |= await ColumnExistsAsync(connection, "InventoryItems", column);

    Console.WriteLine($"UnitCode: {(unitCode ? "PRESENT" : "MISSING")}");
    Console.WriteLine($"Forbidden InventoryItem columns: {(forbiddenPresent ? "PRESENT (FAIL)" : "ABSENT")}");
    return unitCode && !forbiddenPresent;
}

static async Task<bool> ValidateDocumentsAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- DOCUMENTS / TRANSACTIONS ---");
    var pass = true;
    pass &= await ValidateColumnPrecisionAsync(connection, "StockReceipts", "TotalAmount", "numeric", 18, 2);
    pass &= await ValidateColumnPrecisionAsync(connection, "StockReceiptItems", "Quantity", "numeric", 18, 3);
    pass &= await ValidateColumnPrecisionAsync(connection, "StockReceiptItems", "UnitPrice", "numeric", 18, 2);
    pass &= await ValidateColumnPrecisionAsync(connection, "StockIssueItems", "Quantity", "numeric", 18, 3);
    pass &= await ValidateColumnPrecisionAsync(connection, "StockTransactions", "Quantity", "numeric", 18, 3);
    pass &= await ValidateColumnPrecisionAsync(connection, "StockTransactions", "BeforeQuantity", "numeric", 18, 3);
    pass &= await ValidateColumnPrecisionAsync(connection, "StockTransactions", "AfterQuantity", "numeric", 18, 3);

    var receiptStatus = await CheckConstraintExistsAsync(connection, "StockReceipts", "CK_StockReceipts_Status");
    var receiptTotal = await CheckConstraintExistsAsync(connection, "StockReceipts", "CK_StockReceipts_TotalAmount_NonNegative");
    var issueStatus = await CheckConstraintExistsAsync(connection, "StockIssues", "CK_StockIssues_Status");
    var receiptQuantity = await CheckConstraintExistsAsync(connection, "StockReceiptItems", "CK_StockReceiptItems_Quantity_Positive");
    var receiptPrice = await CheckConstraintExistsAsync(connection, "StockReceiptItems", "CK_StockReceiptItems_UnitPrice_NonNegative");
    var issueQuantity = await CheckConstraintExistsAsync(connection, "StockIssueItems", "CK_StockIssueItems_Quantity_Positive");
    var transactionQuantity = await CheckConstraintExistsAsync(connection, "StockTransactions", "CK_StockTransactions_Quantity_Positive");
    var transactionBefore = await CheckConstraintExistsAsync(connection, "StockTransactions", "CK_StockTransactions_BeforeQuantity_NonNegative");
    var transactionAfter = await CheckConstraintExistsAsync(connection, "StockTransactions", "CK_StockTransactions_AfterQuantity_NonNegative");
    var transactionType = await CheckConstraintExistsAsync(connection, "StockTransactions", "CK_StockTransactions_Type");
    var transactionReference = await CheckConstraintExistsAsync(connection, "StockTransactions", "CK_StockTransactions_ReferenceType");

    pass &= receiptStatus && receiptTotal && issueStatus && receiptQuantity && receiptPrice && issueQuantity;
    pass &= transactionQuantity && transactionBefore && transactionAfter && transactionType && transactionReference;

    var referenceFk = await ForeignKeyOnColumnExistsAsync(connection, "StockTransactions", "ReferenceId");
    Console.WriteLine($"ReferenceId polymorphic FK: {(referenceFk ? "PRESENT (FAIL)" : "ABSENT")}");
    Console.WriteLine($"StockReceipt status check: {(receiptStatus ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockReceipt total check: {(receiptTotal ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockIssue status check: {(issueStatus ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockReceiptItem quantity check: {(receiptQuantity ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockReceiptItem price check: {(receiptPrice ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockIssueItem quantity check: {(issueQuantity ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockTransaction quantity check: {(transactionQuantity ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockTransaction before check: {(transactionBefore ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockTransaction after check: {(transactionAfter ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockTransaction type check: {(transactionType ? "PASS" : "FAIL")}");
    Console.WriteLine($"StockTransaction reference check: {(transactionReference ? "PASS" : "FAIL")}");
    Console.WriteLine($"Receipt/issue/transaction validation: {(pass && !referenceFk ? "PASS" : "FAIL")}");
    return pass && !referenceFk;
}

static async Task<bool> ValidateEmployeePreservationAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- EMPLOYEE PRESERVATION ---");
    var employeeType = await ColumnExistsAsync(connection, "Employees", "EmployeeType");
    var basicSalary = await ColumnExistsAsync(connection, "Employees", "BasicSalary");
    var role = await ColumnExistsAsync(connection, "Employees", "Role");
    Console.WriteLine($"Employees.EmployeeType: {(employeeType ? "PRESENT" : "MISSING")}");
    Console.WriteLine($"Employees.BasicSalary: {(basicSalary ? "PRESENT" : "MISSING")}");
    Console.WriteLine($"Employees.Role: {(role ? "PRESENT" : "MISSING")}");
    return employeeType && basicSalary && role;
}

static async Task<bool> ValidatePayrollAbsenceAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- PAYROLL REGRESSION ---");
    var payrollTables = new[] { "Payrolls", "PayrollSettings", "PayrollAdjustments", "EmployeeSalaryProfiles" };
    var unexpected = false;
    foreach (var table in payrollTables)
    {
        var exists = await TableExistsAsync(connection, table);
        unexpected |= exists;
        Console.WriteLine($"{table}: {(exists ? "PRESENT (UNEXPECTED)" : "ABSENT")}");
    }

    return !unexpected;
}

static async Task<bool> ValidateColumnPrecisionAsync(NpgsqlConnection connection, string table, string column, string dataType, int precision, int scale)
{
    var value = await ReadColumnAsync(connection, table, column);
    var pass = value is not null && value.Value.DataType == dataType && value.Value.NumericPrecision == precision && value.Value.NumericScale == scale;
    Console.WriteLine($"{table}.{column} precision: {(pass ? "PASS" : "FAIL")}");
    return pass;
}

static async Task<bool> TableExistsAsync(NpgsqlConnection connection, string tableName) =>
    await ScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = $1
        )
        """, tableName);

static async Task<bool> ColumnExistsAsync(NpgsqlConnection connection, string tableName, string columnName) =>
    await ScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = $1 AND column_name = $2
        )
        """, tableName, columnName);

static async Task<bool> ForeignKeyExistsAsync(NpgsqlConnection connection, string tableName, string constraintName) =>
    await ScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'public' AND t.relname = $1
              AND c.conname = $2 AND c.contype = 'f'
        )
        """, tableName, constraintName);

static async Task<bool> ForeignKeyOnColumnExistsAsync(NpgsqlConnection connection, string tableName, string columnName) =>
    await ScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1
            FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            JOIN unnest(c.conkey) AS key_column(attnum) ON TRUE
            JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = key_column.attnum
            WHERE n.nspname = 'public' AND t.relname = $1
              AND c.contype = 'f' AND a.attname = $2
        )
        """, tableName, columnName);

static async Task<bool> UniqueIndexExistsAsync(NpgsqlConnection connection, string tableName, string indexName) =>
    await ScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM pg_indexes
            WHERE schemaname = 'public' AND tablename = $1 AND indexname = $2
              AND indexdef ILIKE '%UNIQUE%'
        )
        """, tableName, indexName);

static async Task<bool> CheckConstraintExistsAsync(NpgsqlConnection connection, string tableName, string constraintName)
{
    var definition = await ScalarAsync<string?>(connection, """
        SELECT pg_get_constraintdef(c.oid)
        FROM pg_constraint c
        JOIN pg_class t ON t.oid = c.conrelid
        JOIN pg_namespace n ON n.oid = t.relnamespace
        WHERE n.nspname = 'public' AND t.relname = $1
          AND c.conname = $2 AND c.contype = 'c'
        """, tableName, constraintName);

    var pass = definition is not null;
    Console.WriteLine($"{tableName}.{constraintName}: {(pass ? "PASS" : "FAIL")}");
    return pass;
}

static async Task ReportCheckConstraintsAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- CHECK CONSTRAINTS ---");
    await using var command = new NpgsqlCommand("""
        SELECT t.relname, c.conname, pg_get_constraintdef(c.oid)
        FROM pg_constraint c
        JOIN pg_class t ON t.oid = c.conrelid
        JOIN pg_namespace n ON n.oid = t.relnamespace
        WHERE c.contype = 'c'
          AND n.nspname = 'public'
          AND t.relname IN (
              'BranchInventories', 'StockReceipts', 'StockIssues',
              'StockReceiptItems', 'StockIssueItems', 'StockTransactions'
          )
        ORDER BY t.relname, c.conname
        """, connection);
    await using var reader = await command.ExecuteReaderAsync();
    var currentTable = string.Empty;
    while (await reader.ReadAsync())
    {
        var table = reader.GetString(0);
        if (!string.Equals(currentTable, table, StringComparison.Ordinal))
        {
            currentTable = table;
            Console.WriteLine($"--- CHECK CONSTRAINTS: {table} ---");
        }

        Console.WriteLine($"{reader.GetString(1)}: {reader.GetString(2)}");
    }
}

static async Task<ColumnInfo?> ReadColumnAsync(NpgsqlConnection connection, string tableName, string columnName)
{
    await using var command = new NpgsqlCommand("""
        SELECT data_type, udt_name, numeric_precision, numeric_scale, is_nullable
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = $1 AND column_name = $2
        """, connection);
    command.Parameters.AddWithValue(tableName);
    command.Parameters.AddWithValue(columnName);
    await using var reader = await command.ExecuteReaderAsync();
    if (!await reader.ReadAsync())
        return null;

    return new ColumnInfo(
        reader.GetString(0),
        reader.GetString(1),
        reader.IsDBNull(2) ? null : reader.GetInt32(2),
        reader.IsDBNull(3) ? null : reader.GetInt32(3),
        string.Equals(reader.GetString(4), "YES", StringComparison.OrdinalIgnoreCase));
}

static async Task<T> ScalarAsync<T>(NpgsqlConnection connection, string sql, params object?[] parameters)
{
    await using var command = new NpgsqlCommand(sql, connection);
    foreach (var parameter in parameters)
        command.Parameters.AddWithValue(parameter ?? DBNull.Value);

    var value = await command.ExecuteScalarAsync();
    if (value is null or DBNull)
        return default!;
    return (T)Convert.ChangeType(value, typeof(T));
}

static NpgsqlConnectionStringBuilder NormalizeConnectionString(string raw)
{
    var value = raw.Trim();
    if (value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
            throw new UriFormatException("The PostgreSQL connection URI is invalid.");

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = Uri.UnescapeDataString(userInfo[1]),
            SslMode = SslMode.Require
        };
    }

    return new NpgsqlConnectionStringBuilder(value);
}

readonly record struct ColumnInfo(
    string DataType,
    string UdtName,
    int? NumericPrecision,
    int? NumericScale,
    bool IsNullable);
