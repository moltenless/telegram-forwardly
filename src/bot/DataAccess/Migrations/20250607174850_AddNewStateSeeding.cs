using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNewStateSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "client_current_states",
                columns: new[] { "id", "value" },
                values: new object[] { 11L, "AwaitingDeleteConfirmation" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 11L);
        }
    }
}
