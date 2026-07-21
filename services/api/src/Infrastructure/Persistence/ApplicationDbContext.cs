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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình các bảng nếu cần
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Details)
                .WithOne()
                .HasForeignKey("OrderId");
        }
    }
}
