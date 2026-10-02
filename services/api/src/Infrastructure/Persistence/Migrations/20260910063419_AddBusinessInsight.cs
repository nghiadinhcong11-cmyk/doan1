using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessInsight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    AiExplanation = table.Column<string>(type: "text", nullable: true),
                    AiRecommendation = table.Column<string>(type: "text", nullable: true),
                    EvidenceJson = table.Column<string>(type: "text", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ComparisonPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComparisonPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DeduplicationKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessInsights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInsights_BranchId",
                table: "BusinessInsights",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInsights_CreatedAt",
                table: "BusinessInsights",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInsights_DeduplicationKey",
                table: "BusinessInsights",
                column: "DeduplicationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInsights_Status",
                table: "BusinessInsights",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessInsights");
        }
    }
}
