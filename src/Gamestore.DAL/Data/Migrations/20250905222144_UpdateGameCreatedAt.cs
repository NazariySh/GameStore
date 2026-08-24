using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Data.Migrations;

/// <inheritdoc />
public partial class UpdateGameCreatedAt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ReleaseDate",
            table: "Games",
            newName: "CreatedAt");

        migrationBuilder.RenameIndex(
            name: "IX_Games_ReleaseDate",
            table: "Games",
            newName: "IX_Games_CreatedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "CreatedAt",
            table: "Games",
            newName: "ReleaseDate");

        migrationBuilder.RenameIndex(
            name: "IX_Games_CreatedAt",
            table: "Games",
            newName: "IX_Games_ReleaseDate");
    }
}
