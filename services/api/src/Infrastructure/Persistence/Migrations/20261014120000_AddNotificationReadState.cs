using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RestaurantPOS.Infrastructure.Persistence;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261014120000_AddNotificationReadState")]
public partial class AddNotificationReadState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "UseLegacyReadState",
            table: "Notifications",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        // Existing rows retain the historical shared IsRead meaning. New rows
        // use NotificationReads for per-user state after this migration.
        migrationBuilder.Sql("UPDATE \"Notifications\" SET \"UseLegacyReadState\" = TRUE");

        migrationBuilder.CreateTable(
            name: "NotificationReads",
            columns: table => new
            {
                NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NotificationReads", x => new { x.NotificationId, x.UserId });
                table.ForeignKey(
                    name: "FK_NotificationReads_Notifications_NotificationId",
                    column: x => x.NotificationId,
                    principalTable: "Notifications",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_NotificationReads_UserId",
            table: "NotificationReads",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "NotificationReads");
        migrationBuilder.DropColumn(name: "UseLegacyReadState", table: "Notifications");
    }
}
