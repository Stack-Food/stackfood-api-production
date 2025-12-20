using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace StackFood.Production.Infrastructure.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    items_json = table.Column<string>(type: "jsonb", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    estimated_time = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ready_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_production_order_id",
                table: "production_orders",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_production_queue",
                table: "production_orders",
                columns: new[] { "status", "priority", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_production_status",
                table: "production_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_production_status_created",
                table: "production_orders",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "production_orders");
        }
    }
}
