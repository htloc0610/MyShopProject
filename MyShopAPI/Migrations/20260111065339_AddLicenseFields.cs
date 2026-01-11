using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentLicenseKey",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLicensed",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLicenseKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsLicensed",
                table: "AspNetUsers");
        }
    }
}
