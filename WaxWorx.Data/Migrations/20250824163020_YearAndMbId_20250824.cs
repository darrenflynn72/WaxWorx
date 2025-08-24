using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaxWorx.Data.Migrations
{
    /// <inheritdoc />
    public partial class YearAndMbId_20250824 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Images",
                newName: "CoverUrl");

            migrationBuilder.AddColumn<string>(
                name: "MbId",
                table: "Artists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MbId",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReleaseYear",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MbId",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MbId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "Albums");

            migrationBuilder.RenameColumn(
                name: "CoverUrl",
                table: "Images",
                newName: "FileName");
        }
    }
}
