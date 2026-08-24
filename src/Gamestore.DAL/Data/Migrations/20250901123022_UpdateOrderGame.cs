using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable S4581

#nullable disable

namespace Gamestore.DAL.Data.Migrations;

/// <inheritdoc />
public partial class UpdateOrderGame : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "Id",
            table: "OrderGames",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddUniqueConstraint(
            name: "AK_OrderGames_Id",
            table: "OrderGames",
            column: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropUniqueConstraint(
            name: "AK_OrderGames_Id",
            table: "OrderGames");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "OrderGames");
    }
}