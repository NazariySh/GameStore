using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Data.Migrations;

/// <inheritdoc />
public partial class UpdateGame : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "CommentCount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "ReleaseDate",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<int>(
            name: "ViewCount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Games_Price",
            table: "Games",
            column: "Price");

        migrationBuilder.CreateIndex(
            name: "IX_Games_ReleaseDate",
            table: "Games",
            column: "ReleaseDate");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Games_Price",
            table: "Games");

        migrationBuilder.DropIndex(
            name: "IX_Games_ReleaseDate",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "CommentCount",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ReleaseDate",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ViewCount",
            table: "Games");
    }
}