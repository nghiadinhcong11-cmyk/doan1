using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RestaurantPOS.Infrastructure.Persistence;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261027000000_RepairReceiptSettingsColumns")]
partial class RepairReceiptSettingsColumns
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.31")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);
    }
}
