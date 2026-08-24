using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable SA1413
#pragma warning disable S4581

#nullable disable

namespace Gamestore.DAL.Data.Migrations;

/// <inheritdoc />
public partial class AddPublisher : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Discount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<decimal>(
            name: "Price",
            table: "Games",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<Guid>(
            name: "PublisherId",
            table: "Games",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<int>(
            name: "UnitInStock",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "Publishers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                HomePage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Publishers", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Games_PublisherId",
            table: "Games",
            column: "PublisherId");

        migrationBuilder.CreateIndex(
            name: "IX_Publishers_CompanyName",
            table: "Publishers",
            column: "CompanyName",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Games_Publishers_PublisherId",
            table: "Games",
            column: "PublisherId",
            principalTable: "Publishers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Games_Publishers_PublisherId",
            table: "Games");

        migrationBuilder.DropTable(
            name: "Publishers");

        migrationBuilder.DropIndex(
            name: "IX_Games_PublisherId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Discount",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Price",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "PublisherId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "UnitInStock",
            table: "Games");
    }
}
