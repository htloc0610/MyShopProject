using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSellingPriceToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellingPrice",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "Products");
        }
    }
}
