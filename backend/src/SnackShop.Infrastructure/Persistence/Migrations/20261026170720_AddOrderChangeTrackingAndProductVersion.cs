using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnackShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderChangeTrackingAndProductVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "products",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_changed_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            // DATA MIGRATION: existing orders get a meaningful value instead of year 0001.
            migrationBuilder.Sql("UPDATE orders SET last_changed_at = COALESCE(delivered_at, cancelled_at, shipped_at, confirmed_at, placed_at);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "products");

            migrationBuilder.DropColumn(
                name: "last_changed_at",
                table: "orders");
        }
    }
}
