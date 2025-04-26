using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaret.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToProductsAndUsers2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$10$C9Pgk8HTi2WBj5tUIp8F8epdl00eEWFBYmmoLU9Nxc/IopWpLjxKG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Y9b/JqK8cF5dzOJqX3Nk8eGfTzAWM5gP8EMHEQ4Tdo6V2Qe8z5H.O");
        }
    }
}
