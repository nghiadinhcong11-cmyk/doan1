using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBranchAvailability> ProductBranchAvailabilities { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ManualExpenseIdempotencyRecord> ManualExpenseIdempotencyRecords { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<RestaurantTable> Tables { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Topping> Toppings { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<OrderRequest> OrderRequests { get; set; }
        public DbSet<OrderRequestItem> OrderRequestItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRead> NotificationReads { get; set; }
        public DbSet<ReceiptSettings> ReceiptSettings { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<BranchInventory> BranchInventories { get; set; }
        public DbSet<StockReceipt> StockReceipts { get; set; }
        public DbSet<StockReceiptItem> StockReceiptItems { get; set; }
        public DbSet<StockIssue> StockIssues { get; set; }
        public DbSet<StockIssueItem> StockIssueItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Phase 1 domain relationship refinement. Nullable FKs preserve legacy
            // rows until the deterministic backfill has been verified.
            modelBuilder.Entity<Area>()
                .HasOne(a => a.Branch)
                .WithMany()
                .HasForeignKey(a => a.BranchId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Area>()
                .HasIndex(a => a.BranchId);

            // 1. Hóa đơn và Chi tiết hóa đơn
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Details)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Branch)
                .WithMany()
                .HasForeignKey(o => o.BranchId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            // 1.1 Order và OrderRequest
            modelBuilder.Entity<OrderRequest>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(r => r.OrderId);

            // 1.2 OrderRequest và OrderRequestItem
            modelBuilder.Entity<OrderRequest>()
                .HasMany(r => r.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Chi tiết hóa đơn liên kết với Sản phẩm hoặc Topping
            modelBuilder.Entity<OrderDetail>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .IsRequired(false);

            modelBuilder.Entity<OrderDetail>()
                .HasOne<Topping>()
                .WithMany()
                .HasForeignKey(d => d.ToppingId)
                .IsRequired(false);

            modelBuilder.Entity<ProductBranchAvailability>(entity =>
            {
                entity.ToTable("ProductBranchAvailabilities");
                entity.HasKey(state => new { state.ProductId, state.BranchId });
                entity.Property(state => state.Status).HasMaxLength(32).IsRequired();
                entity.Property(state => state.UpdatedAtUtc).IsRequired();
                entity.HasOne(state => state.Product)
                    .WithMany()
                    .HasForeignKey(state => state.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(state => state.Branch)
                    .WithMany()
                    .HasForeignKey(state => state.BranchId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(state => new { state.BranchId, state.Status });
            });

            // 3. Nhân viên liên kết với Chi nhánh
            modelBuilder.Entity<Employee>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(e => e.BranchId);

            // 4. Bàn liên kết với Chi nhánh
            modelBuilder.Entity<RestaurantTable>()
                .HasOne(t => t.Branch)
                .WithMany()
                .HasForeignKey(t => t.BranchId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<RestaurantTable>()
                .HasOne(t => t.Area)
                .WithMany()
                .HasForeignKey(t => t.AreaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.AreaId);
            modelBuilder.Entity<RestaurantTable>()
                .Property(t => t.QrToken)
                .IsRequired()
                .HasMaxLength(43);
            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.QrToken)
                .IsUnique();

            // 5. Chấm công liên kết với Nhân viên và Chi nhánh
            modelBuilder.Entity<Attendance>()
                .HasOne<Employee>()
                .WithMany()
                .HasForeignKey(a => a.EmployeeId);
            modelBuilder.Entity<Attendance>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(a => a.BranchId);

            // 6. Lịch làm việc liên kết với Nhân viên và Chi nhánh
            modelBuilder.Entity<WorkSchedule>()
                .HasOne<Employee>()
                .WithMany()
                .HasForeignKey(s => s.EmployeeId);
            modelBuilder.Entity<WorkSchedule>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(s => s.BranchId);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Branch)
                .WithMany()
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Branch)
                .WithMany()
                .HasForeignKey(r => r.BranchId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany()
                .HasForeignKey(r => r.TableId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Expense>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Expense>()
                .HasIndex(e => new { e.BranchId, e.ExpenseDate });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.TargetRole, n.TargetUserId, n.CreatedAt });
            modelBuilder.Entity<NotificationRead>()
                .HasKey(read => new { read.NotificationId, read.UserId });
            modelBuilder.Entity<NotificationRead>()
                .HasIndex(read => read.UserId);
            modelBuilder.Entity<NotificationRead>()
                .HasOne(read => read.Notification)
                .WithMany()
                .HasForeignKey(read => read.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ReceiptSettings>()
                .HasIndex(s => s.BranchId)
                .IsUnique()
                .HasFilter("\"BranchId\" IS NOT NULL");
            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => new { s.BranchId, s.Key })
                .IsUnique()
                .HasFilter("\"BranchId\" IS NOT NULL");

            // 7. Khách hàng và Tích điểm
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber).IsUnique();

            // Analytics Indexes
            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.BranchId, o.CreatedAt, o.Status });
            modelBuilder.Entity<LoyaltyTransaction>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(t => t.CustomerId);
            modelBuilder.Entity<LoyaltyTransaction>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(t => t.OrderId)
                .IsRequired(false);
            modelBuilder.Entity<LoyaltyTransaction>()
                .HasIndex(t => new { t.CustomerId, t.CreatedAt });
            modelBuilder.Entity<LoyaltyTransaction>()
                .HasIndex(t => new { t.OrderId, t.Type }).IsUnique()
                .HasFilter("\"OrderId\" IS NOT NULL");

            ConfigureInventory(modelBuilder);

            // Order -> Customer relationship
            modelBuilder.Entity<Order>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .IsRequired(false);
        }

        private static void ConfigureInventory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.ToTable("InventoryItems");
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
                entity.Property(item => item.NormalizedName).HasMaxLength(200).IsRequired();
                entity.Property(item => item.UnitCode).HasMaxLength(20).IsRequired();
                entity.HasIndex(item => item.NormalizedName)
                    .IsUnique()
                    .HasFilter("\"IsActive\" = TRUE");
            });

            modelBuilder.Entity<BranchInventory>(entity =>
            {
                entity.ToTable("BranchInventories", table =>
                {
                    table.HasCheckConstraint("CK_BranchInventories_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0");
                    table.HasCheckConstraint("CK_BranchInventories_MinimumStock_NonNegative", "\"MinimumStock\" >= 0");
                });
                entity.HasKey(item => item.Id);
                entity.Property(item => item.CurrentQuantity).HasPrecision(18, 3);
                entity.Property(item => item.MinimumStock).HasPrecision(18, 3);
                entity.HasIndex(item => new { item.BranchId, item.InventoryItemId }).IsUnique();
                entity.HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.InventoryItem).WithMany(item => item.BranchInventories).HasForeignKey(item => item.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockReceipt>(entity =>
            {
                entity.ToTable("StockReceipts", table =>
                {
                    table.HasCheckConstraint("CK_StockReceipts_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
                    table.HasCheckConstraint("CK_StockReceipts_Status", "\"Status\" IN ('Draft', 'Confirmed', 'Cancelled')");
                });
                entity.HasKey(receipt => receipt.Id);
                entity.Property(receipt => receipt.SupplierName).HasMaxLength(200);
                entity.Property(receipt => receipt.TotalAmount).HasPrecision(18, 2);
                entity.Property(receipt => receipt.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
                entity.Property(receipt => receipt.IdempotencyKey).HasMaxLength(100);
                entity.HasIndex(receipt => new { receipt.BranchId, receipt.CreatedAtUtc });
                entity.HasIndex(receipt => new { receipt.BranchId, receipt.IdempotencyKey })
                    .IsUnique()
                    .HasFilter("\"IdempotencyKey\" IS NOT NULL");
                entity.HasOne(receipt => receipt.Branch).WithMany().HasForeignKey(receipt => receipt.BranchId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(receipt => receipt.Items).WithOne(item => item.StockReceipt).HasForeignKey(item => item.StockReceiptId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockReceiptItem>(entity =>
            {
                entity.ToTable("StockReceiptItems", table =>
                {
                    table.HasCheckConstraint("CK_StockReceiptItems_Quantity_Positive", "\"Quantity\" > 0");
                    table.HasCheckConstraint("CK_StockReceiptItems_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
                });
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Quantity).HasPrecision(18, 3);
                entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
                entity.HasIndex(item => new { item.StockReceiptId, item.InventoryItemId });
                entity.HasOne(item => item.InventoryItem).WithMany().HasForeignKey(item => item.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockIssue>(entity =>
            {
                entity.ToTable("StockIssues", table =>
                {
                    table.HasCheckConstraint("CK_StockIssues_Status", "\"Status\" IN ('Draft', 'Confirmed', 'Cancelled')");
                });
                entity.HasKey(issue => issue.Id);
                entity.Property(issue => issue.Reason).HasMaxLength(500).IsRequired();
                entity.Property(issue => issue.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
                entity.Property(issue => issue.IdempotencyKey).HasMaxLength(100);
                entity.HasIndex(issue => new { issue.BranchId, issue.CreatedAtUtc });
                entity.HasIndex(issue => new { issue.BranchId, issue.IdempotencyKey })
                    .IsUnique()
                    .HasFilter("\"IdempotencyKey\" IS NOT NULL");
                entity.HasOne(issue => issue.Branch).WithMany().HasForeignKey(issue => issue.BranchId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(issue => issue.Items).WithOne(item => item.StockIssue).HasForeignKey(item => item.StockIssueId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockIssueItem>(entity =>
            {
                entity.ToTable("StockIssueItems", table =>
                {
                    table.HasCheckConstraint("CK_StockIssueItems_Quantity_Positive", "\"Quantity\" > 0");
                });
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Quantity).HasPrecision(18, 3);
                entity.HasIndex(item => new { item.StockIssueId, item.InventoryItemId });
                entity.HasOne(item => item.InventoryItem).WithMany().HasForeignKey(item => item.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.ToTable("StockTransactions", table =>
                {
                    table.HasCheckConstraint("CK_StockTransactions_Quantity_Positive", "\"Quantity\" > 0");
                    table.HasCheckConstraint("CK_StockTransactions_BeforeQuantity_NonNegative", "\"BeforeQuantity\" >= 0");
                    table.HasCheckConstraint("CK_StockTransactions_AfterQuantity_NonNegative", "\"AfterQuantity\" >= 0");
                    table.HasCheckConstraint("CK_StockTransactions_Type", "\"Type\" IN ('IN', 'OUT', 'ADJUSTMENT')");
                    table.HasCheckConstraint("CK_StockTransactions_ReferenceType", "\"ReferenceType\" IN ('StockReceipt', 'StockIssue', 'StockAdjustment')");
                });
                entity.HasKey(transaction => transaction.Id);
                entity.Property(transaction => transaction.Type)
                    .HasConversion<string>()
                    .HasMaxLength(12)
                    .IsRequired();
                entity.Property(transaction => transaction.Quantity).HasPrecision(18, 3);
                entity.Property(transaction => transaction.BeforeQuantity).HasPrecision(18, 3);
                entity.Property(transaction => transaction.AfterQuantity).HasPrecision(18, 3);
                entity.Property(transaction => transaction.ReferenceType).HasMaxLength(30).IsRequired();
                entity.HasIndex(transaction => new { transaction.BranchId, transaction.InventoryItemId, transaction.CreatedAtUtc });
                entity.HasIndex(transaction => new { transaction.ReferenceType, transaction.ReferenceId });
                entity.HasOne(transaction => transaction.Branch).WithMany().HasForeignKey(transaction => transaction.BranchId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(transaction => transaction.InventoryItem).WithMany().HasForeignKey(transaction => transaction.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(expense => expense.PaymentMethod).IsRequired();
                entity.Property(expense => expense.Note).IsRequired();
                entity.HasOne(expense => expense.StockReceipt)
                    .WithOne()
                    .HasForeignKey<Expense>(expense => expense.StockReceiptId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(expense => expense.StockReceiptId)
                    .IsUnique()
                    .HasFilter("\"StockReceiptId\" IS NOT NULL");
            });

            modelBuilder.Entity<ManualExpenseIdempotencyRecord>(entity =>
            {
                entity.ToTable("ManualExpenseIdempotencyRecords");
                entity.HasKey(record => record.Id);
                entity.Property(record => record.IdempotencyKey).HasMaxLength(128).IsRequired();
                entity.Property(record => record.PayloadHash).HasMaxLength(64).IsRequired();
                entity.Property(record => record.ResponseJson).IsRequired();
                entity.HasIndex(record => new { record.BranchId, record.UserId, record.IdempotencyKey }).IsUnique();
                entity.HasIndex(record => record.CreatedAtUtc);
                entity.HasOne<Branch>().WithMany().HasForeignKey(record => record.BranchId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
