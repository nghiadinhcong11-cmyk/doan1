using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

public partial class AddKitchenHistoryAndNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("AvailabilityStatus", "Products", type: "text", nullable: false, defaultValue: "Available");
        migrationBuilder.AddColumn<DateTime>("AcceptedAt", "OrderRequests", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<DateTime>("PreparingAt", "OrderRequests", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<DateTime>("CompletedAt", "OrderRequests", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>("ProcessedBy", "OrderRequests", type: "text", nullable: true);
        migrationBuilder.CreateTable(name: "Notifications", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), Type = table.Column<string>(type: "text", nullable: false),
            Title = table.Column<string>(type: "text", nullable: false), Message = table.Column<string>(type: "text", nullable: false),
            EntityType = table.Column<string>(type: "text", nullable: true), EntityId = table.Column<Guid>(type: "uuid", nullable: true),
            Route = table.Column<string>(type: "text", nullable: true), TargetRole = table.Column<string>(type: "text", nullable: true),
            TargetUserId = table.Column<Guid>(type: "uuid", nullable: true), BranchId = table.Column<Guid>(type: "uuid", nullable: true),
            IsRead = table.Column<bool>(type: "boolean", nullable: false), CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Notifications", x => x.Id));
        migrationBuilder.CreateIndex("IX_Notifications_TargetRole_TargetUserId_CreatedAt", "Notifications", new[] { "TargetRole", "TargetUserId", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Notifications");
        migrationBuilder.DropColumn("AvailabilityStatus", "Products");
        migrationBuilder.DropColumn("AcceptedAt", "OrderRequests"); migrationBuilder.DropColumn("PreparingAt", "OrderRequests");
        migrationBuilder.DropColumn("CompletedAt", "OrderRequests"); migrationBuilder.DropColumn("ProcessedBy", "OrderRequests");
    }
}
