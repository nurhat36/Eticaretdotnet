using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaret.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToProductsAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.AddColumn<DateTime>(
               name: "CreatedAt",
               table: "Users",
               type: "datetime2",
               nullable: false,
               defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));


            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Role", "UserName" },
                values: new object[] { 1, new DateTime(2025, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@eticaret.com", "$2a$11$Y9b/JqK8cF5dzOJqX3Nk8eGfTzAWM5gP8EMHEQ4Tdo6V2Qe8z5H.O", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");
        }
    }
}
