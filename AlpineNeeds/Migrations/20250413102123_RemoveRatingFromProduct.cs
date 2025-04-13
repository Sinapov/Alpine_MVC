﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlpineNeeds.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRatingFromProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Products",
                type: "decimal(3,1)",
                nullable: true);
        }
    }
}
