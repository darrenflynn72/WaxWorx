using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaxWorx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlbumDatePurchased_20250825 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePurchased",
                table: "Albums",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePurchased",
                table: "Albums");
        }
    }
}
