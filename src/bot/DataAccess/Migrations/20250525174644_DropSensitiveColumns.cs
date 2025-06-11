using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DropSensitiveColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 13L);

            migrationBuilder.DropColumn(
                name: "password",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "verification_code",
                table: "clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "clients",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_code",
                table: "clients",
                type: "varchar(16)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "client_current_states",
                columns: ["id", "value"],
                values: [13L, "Ready"]);
        }
    }
}
