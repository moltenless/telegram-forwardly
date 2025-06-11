using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chat_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    value = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_current_states",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    value = table.Column<string>(type: "varchar(124)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_current_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topic_grouping_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    value = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_grouping_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    current_state_id = table.Column<int>(type: "int", nullable: true),
                    api_id = table.Column<string>(type: "varchar(20)", nullable: true),
                    api_hash = table.Column<string>(type: "varchar(64)", nullable: true),
                    session_sting = table.Column<string>(type: "varchar(600)", nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    verification_code = table.Column<string>(type: "varchar(16)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    registration_datetime = table.Column<DateTime>(type: "datetime", nullable: true),
                    username = table.Column<string>(type: "varchar(32)", nullable: true),
                    first_name = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    forum_supergroup_id = table.Column<long>(type: "bigint", nullable: true),
                    logging_topic_enabled = table.Column<bool>(type: "bit", nullable: true),
                    topic_grouping_type_id = table.Column<int>(type: "int", nullable: true),
                    forwardly_enabled = table.Column<bool>(type: "bit", nullable: true),
                    all_chats_filtering_enabled = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_clients_client_current_states_current_state_id",
                        column: x => x.current_state_id,
                        principalTable: "client_current_states",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_clients_topic_grouping_types_topic_grouping_type_id",
                        column: x => x.topic_grouping_type_id,
                        principalTable: "topic_grouping_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "chats",
                columns: table => new
                {
                    db_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    tg_id = table.Column<long>(type: "bigint", nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.db_id);
                    table.ForeignKey(
                        name: "FK_chats_chat_types_type_id",
                        column: x => x.type_id,
                        principalTable: "chat_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chats_clients_user_id",
                        column: x => x.user_id,
                        principalTable: "clients",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "keywords",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<string>(type: "nvarchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_keywords", x => x.id);
                    table.ForeignKey(
                        name: "FK_keywords_clients_user_id",
                        column: x => x.user_id,
                        principalTable: "clients",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chats_type_id",
                table: "chats",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_user_id",
                table: "chats",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_clients_current_state_id",
                table: "clients",
                column: "current_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_clients_topic_grouping_type_id",
                table: "clients",
                column: "topic_grouping_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_keywords_user_id",
                table: "keywords",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chats");

            migrationBuilder.DropTable(
                name: "keywords");

            migrationBuilder.DropTable(
                name: "chat_types");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "client_current_states");

            migrationBuilder.DropTable(
                name: "topic_grouping_types");
        }
    }
}
