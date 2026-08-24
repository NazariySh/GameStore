using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.DAL.Data.Migrations;

/// <inheritdoc />
public partial class AddNorthwindFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Comments_Games_GameId",
            table: "Comments");

        migrationBuilder.DropForeignKey(
            name: "FK_OrderGames_Games_ProductId",
            table: "OrderGames");

        migrationBuilder.DropIndex(
            name: "IX_OrderGames_ProductId",
            table: "OrderGames");

        migrationBuilder.DropIndex(
            name: "IX_Comments_GameId",
            table: "Comments");

        migrationBuilder.AddColumn<string>(
            name: "Address",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "City",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ContactName",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ContactTitle",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Country",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Fax",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PostalCode",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Region",
            table: "Publishers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SupplierId",
            table: "Publishers",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "EmployeeId",
            table: "Orders",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<double>(
            name: "Freight",
            table: "Orders",
            type: "float",
            nullable: false,
            defaultValue: 0.0);

        migrationBuilder.AddColumn<string>(
            name: "MongoCustomerId",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "MongoOrderId",
            table: "Orders",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RequiredDate",
            table: "Orders",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipAddress",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipCity",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipCountry",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipName",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipPostalCode",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipRegion",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ShipVia",
            table: "Orders",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "ShippedDate",
            table: "Orders",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "MongoOrderId",
            table: "OrderGames",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "MongoProductId",
            table: "OrderGames",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "CategoryId",
            table: "Genres",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Genres",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Picture",
            table: "Genres",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "CategoryId",
            table: "Games",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ProductId",
            table: "Games",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "QuantityPerUnit",
            table: "Games",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ReorderLevel",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "SupplierId",
            table: "Games",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "UnitsOnOrder",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "ProductId",
            table: "Comments",
            type: "int",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Address",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "City",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "ContactName",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "ContactTitle",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "Country",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "Fax",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "PostalCode",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "Region",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "SupplierId",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "EmployeeId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "Freight",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "MongoCustomerId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "MongoOrderId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "RequiredDate",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipAddress",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipCity",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipCountry",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipName",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipPostalCode",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipRegion",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipVia",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShippedDate",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "MongoOrderId",
            table: "OrderGames");

        migrationBuilder.DropColumn(
            name: "MongoProductId",
            table: "OrderGames");

        migrationBuilder.DropColumn(
            name: "CategoryId",
            table: "Genres");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "Genres");

        migrationBuilder.DropColumn(
            name: "Picture",
            table: "Genres");

        migrationBuilder.DropColumn(
            name: "CategoryId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ProductId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "QuantityPerUnit",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ReorderLevel",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "SupplierId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "UnitsOnOrder",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "ProductId",
            table: "Comments");

        migrationBuilder.CreateIndex(
            name: "IX_OrderGames_ProductId",
            table: "OrderGames",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_GameId",
            table: "Comments",
            column: "GameId");

        migrationBuilder.AddForeignKey(
            name: "FK_Comments_Games_GameId",
            table: "Comments",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_OrderGames_Games_ProductId",
            table: "OrderGames",
            column: "ProductId",
            principalTable: "Games",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
