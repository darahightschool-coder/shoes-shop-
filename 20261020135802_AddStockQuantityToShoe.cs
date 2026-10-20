using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeShop.Migrations
{
    /// <inheritdoc />
    public partial class AddStockQuantityToShoe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Shoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Shoes");
        }
    }
}
