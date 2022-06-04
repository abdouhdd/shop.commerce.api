using Microsoft.EntityFrameworkCore.Migrations;

namespace shop.commerce.api.infrastructure.Migrations
{
    public partial class addProductQuantityInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QuantityInitial",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "OrderItemNumber",
                table: "OrderItems",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderItemNumber",
                table: "OrderItems",
                column: "OrderItemNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderItemNumber",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "QuantityInitial",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "OrderItemNumber",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)",
                oldMaxLength: 20);
        }
    }
}
