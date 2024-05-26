using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class checkDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCharts_Products_ProdcutId",
                table: "ShoppingCharts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCharts_ProdcutId",
                table: "ShoppingCharts");

            migrationBuilder.DropColumn(
                name: "ProdcutId",
                table: "ShoppingCharts");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCharts_ProductId",
                table: "ShoppingCharts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCharts_Products_ProductId",
                table: "ShoppingCharts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCharts_Products_ProductId",
                table: "ShoppingCharts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCharts_ProductId",
                table: "ShoppingCharts");

            migrationBuilder.AddColumn<int>(
                name: "ProdcutId",
                table: "ShoppingCharts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCharts_ProdcutId",
                table: "ShoppingCharts",
                column: "ProdcutId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCharts_Products_ProdcutId",
                table: "ShoppingCharts",
                column: "ProdcutId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
