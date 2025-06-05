using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 12L);

            migrationBuilder.DropColumn(
                name: "logging_topic_enabled",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "password",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "verification_code",
                table: "clients");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 5L,
                column: "value",
                value: "AwaitingSessionString");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 6L,
                column: "value",
                value: "AwaitingEnableAllChats");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 7L,
                column: "value",
                value: "AwaitingChats");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 8L,
                column: "value",
                value: "AwaitingKeywords");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 9L,
                column: "value",
                value: "AwaitingForumGroup");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 10L,
                column: "value",
                value: "AwaitingGroupingType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "logging_topic_enabled",
                table: "clients",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "clients",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_code",
                table: "clients",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 5L,
                column: "value",
                value: "AwaitingVerificationCode");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 6L,
                column: "value",
                value: "AwaitingPassword");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 7L,
                column: "value",
                value: "AwaitingEnableAllChats");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 8L,
                column: "value",
                value: "AwaitingChats");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 9L,
                column: "value",
                value: "AwaitingKeywords");

            migrationBuilder.UpdateData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 10L,
                column: "value",
                value: "AwaitingForumGroup");

            migrationBuilder.InsertData(
                table: "client_current_states",
                columns: new[] { "id", "value" },
                values: new object[,]
                {
                    { 11L, "AwaitingGroupingType" },
                    { 12L, "AwaitingEnableLoggingTopic" }
                });
        }
    }
}
