using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlpineNeeds.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductWithColorAndSizeCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Products",
                newName: "Sizes");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Products",
                newName: "Colors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sizes",
                table: "Products",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "Colors",
                table: "Products",
                newName: "Color");
        }
    }
}
