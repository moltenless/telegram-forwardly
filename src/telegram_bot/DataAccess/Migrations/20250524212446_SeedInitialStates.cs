using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "client_current_states",
                columns: ["id", "value"],
                values: new object[,]
                {
                    { 1L, "Idle" },
                    { 2L, "AwaitingPhoneNumber" },
                    { 3L, "AwaitingApiId" },
                    { 4L, "AwaitingApiHash" },
                    { 5L, "AwaitingVerificationCode" },
                    { 6L, "AwaitingPassword" },
                    { 7L, "AwaitingEnableAllChats" },
                    { 8L, "AwaitingChats" },
                    { 9L, "AwaitingKeywords" },
                    { 10L, "AwaitingForumGroup" },
                    { 11L, "AwaitingGroupingType" },
                    { 12L, "AwaitingEnableLoggingTopic" },
                    { 13L, "Ready" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "client_current_states",
                keyColumn: "id",
                keyValue: 13L);
        }
    }
}
