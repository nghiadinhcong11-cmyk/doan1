using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Expense> Expenses { get; set; }
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
        public DbSet<ReceiptSettings> ReceiptSettings { get; set; }
        public DbSet<EmployeeSalaryProfile> EmployeeSalaryProfiles { get; set; }
        public DbSet<PayrollSettings> PayrollSettings { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PayrollAdjustment> PayrollAdjustments { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Hóa đơn và Chi tiết hóa đơn
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Details)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

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

            // 3. Nhân viên liên kết với Chi nhánh
            modelBuilder.Entity<Employee>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(e => e.BranchId);

            // 4. Bàn liên kết với Chi nhánh
            modelBuilder.Entity<RestaurantTable>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(t => t.BranchId);

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

            modelBuilder.Entity<Expense>()
                .HasOne<Branch>()
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Expense>()
                .HasIndex(e => new { e.BranchId, e.ExpenseDate });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.TargetRole, n.TargetUserId, n.CreatedAt });
            modelBuilder.Entity<ReceiptSettings>()
                .HasIndex(s => s.BranchId).IsUnique();
            modelBuilder.Entity<EmployeeSalaryProfile>()
                .HasIndex(s => new { s.EmployeeId, s.BranchId, s.EffectiveFrom });
            modelBuilder.Entity<PayrollSettings>()
                .HasIndex(s => s.BranchId).IsUnique();
            modelBuilder.Entity<Payroll>()
                .HasIndex(p => new { p.EmployeeId, p.BranchId, p.Month, p.Year }).IsUnique();
            modelBuilder.Entity<Payroll>()
                .HasMany(p => p.Adjustments).WithOne().HasForeignKey(a => a.PayrollId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PayrollAdjustment>()
                .HasIndex(a => new { a.PayrollId, a.CreatedAt });
            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => new { s.BranchId, s.Key }).IsUnique();

            // 7. Khách hàng và Tích điểm
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber).IsUnique();
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

            // Order -> Customer relationship
            modelBuilder.Entity<Order>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .IsRequired(false);
        }
    }
}
