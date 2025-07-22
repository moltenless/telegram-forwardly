using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ThresholdCharsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "threshold_chars_count",
                table: "clients",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "client_current_states",
                columns: new[] { "id", "value" },
                values: new object[] { 14L, "AwaitingThresholdCharsCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 14L);

            migrationBuilder.DropColumn(
                name: "threshold_chars_count",
                table: "clients");
        }
    }
}
