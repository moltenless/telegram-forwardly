using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramForwardly.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RefineSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys first to avoid constraint violations when renaming columns.

            migrationBuilder.DropForeignKey(
                name: "FK_chats_chat_types_type_id",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_chats_clients_user_id",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_clients_topic_grouping_types_topic_grouping_type_id",
                table: "clients");

            migrationBuilder.DropForeignKey(
                name: "FK_keywords_clients_user_id",
                table: "keywords");

            migrationBuilder.DropForeignKey(
                name: "FK_clients_client_current_states_current_state_id",
                table: "clients");

            ///Primary key constraints are dropped before renaming columns.

            migrationBuilder.DropPrimaryKey(
                name: "PK_keywords",
                table: "keywords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_client_current_states",
                table: "client_current_states");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chats",
                table: "chats");

            // Drop the tables and indexes that are no longer needed.

            migrationBuilder.DropTable(
                name: "chat_types");

            migrationBuilder.DropTable(
                name: "topic_grouping_types");

            migrationBuilder.DropIndex(
                name: "IX_clients_topic_grouping_type_id",
                table: "clients");

            migrationBuilder.DropIndex(
                name: "IX_chats_type_id",
                table: "chats");

            migrationBuilder.DropIndex(
                name: "IX_chats_user_id",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "topic_grouping_type_id",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "type_id",
                table: "chats");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "keywords",
                newName: "telegram_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_keywords_user_id",
                table: "keywords",
                newName: "IX_keywords_telegram_user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "clients",
                newName: "telegram_user_id");

            migrationBuilder.RenameColumn(
                name: "db_id",
                table: "chats",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "chats",
                newName: "telegram_user_id");

            migrationBuilder.RenameColumn(
                name: "tg_id",
                table: "chats",
                newName: "tg_chat_id");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "keywords",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "current_state_id",
                table: "clients",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_authenticated",
                table: "clients",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "topic_grouping",
                table: "clients",
                type: "varchar(32)",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "client_current_states",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "chats",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_chats_telegram_user_id",
                table: "chats",
                column: "telegram_user_id");

            // Add the primary key constraints back after renaming columns.

            migrationBuilder.AddPrimaryKey(
                name: "PK_keywords",
                table: "keywords",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_client_current_states",
                table: "client_current_states",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chats",
                table: "chats",
                column: "id");

            // Recreate the foreign key constraints with the updated column names.

            migrationBuilder.AddForeignKey(
                name: "FK_clients_client_current_states_current_state_id",
                table: "clients",
                column: "current_state_id",
                principalTable: "client_current_states",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chats_clients_telegram_user_id",
                table: "chats",
                column: "telegram_user_id",
                principalTable: "clients",
                principalColumn: "telegram_user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_keywords_clients_telegram_user_id",
                table: "keywords",
                column: "telegram_user_id",
                principalTable: "clients",
                principalColumn: "telegram_user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys first to avoid constraint violations when renaming columns.

            migrationBuilder.DropForeignKey(
                name: "FK_chats_clients_telegram_user_id",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_keywords_clients_telegram_user_id",
                table: "keywords");

            migrationBuilder.DropForeignKey(
                name: "FK_clients_client_current_states_current_state_id",
                table: "clients");

            // Drop primary key constraints before renaming columns.

            migrationBuilder.DropPrimaryKey(
                name: "PK_keywords",
                table: "keywords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_client_current_states",
                table: "client_current_states");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chats",
                table: "chats");

            // Drop the indexes etc that are no longer needed.

            migrationBuilder.DropIndex(
                name: "IX_chats_telegram_user_id",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "is_authenticated",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "topic_grouping",
                table: "clients");

            migrationBuilder.RenameColumn(
                name: "telegram_user_id",
                table: "keywords",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_keywords_telegram_user_id",
                table: "keywords",
                newName: "IX_keywords_user_id");

            migrationBuilder.RenameColumn(
                name: "telegram_user_id",
                table: "clients",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "chats",
                newName: "db_id");

            migrationBuilder.RenameColumn(
                name: "tg_chat_id",
                table: "chats",
                newName: "tg_id");

            migrationBuilder.RenameColumn(
                name: "telegram_user_id",
                table: "chats",
                newName: "user_id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "keywords",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "current_state_id",
                table: "clients",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "topic_grouping_type_id",
                table: "clients",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "client_current_states",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "db_id",
                table: "chats",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "type_id",
                table: "chats",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_clients_topic_grouping_type_id",
                table: "clients",
                column: "topic_grouping_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_type_id",
                table: "chats",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_user_id",
                table: "chats",
                column: "user_id");

            // Recreate the PRIMARY key constraints with the original column names.

            migrationBuilder.AddPrimaryKey(
                name: "PK_keywords",
                table: "keywords",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_client_current_states",
                table: "client_current_states",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chats",
                table: "chats",
                column: "db_id");

            // Recreate the foreign key constraints with the original column names.

            migrationBuilder.AddForeignKey(
                name: "FK_clients_client_current_states_current_state_id",
                table: "clients",
                column: "current_state_id",
                principalTable: "client_current_states",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chats_chat_types_type_id",
                table: "chats",
                column: "type_id",
                principalTable: "chat_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chats_clients_user_id",
                table: "chats",
                column: "user_id",
                principalTable: "clients",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_clients_topic_grouping_types_topic_grouping_type_id",
                table: "clients",
                column: "topic_grouping_type_id",
                principalTable: "topic_grouping_types",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_keywords_clients_user_id",
                table: "keywords",
                column: "user_id",
                principalTable: "clients",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
