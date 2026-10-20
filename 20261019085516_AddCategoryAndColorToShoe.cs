using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeShop.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndColorToShoe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Shoes",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Shoes",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Shoes");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Shoes");
        }
    }
}
