using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thrume.Database.Migrations
{
    /// <inheritdoc />
    public partial class MessagesFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversation_db_set",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation_db_set", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_conversation",
                columns: table => new
                {
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participants_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_conversation", x => new { x.conversation_id, x.participants_id });
                    table.ForeignKey(
                        name: "fk_account_conversation_accounts_participants_id",
                        column: x => x.participants_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_account_conversation_conversation_db_set_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversation_db_set",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_db_set",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_db_set", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_db_set_accounts_sender_id",
                        column: x => x.sender_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_message_db_set_conversation_db_set_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversation_db_set",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_conversation_participants_id",
                table: "account_conversation",
                column: "participants_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_db_set_conversation_id",
                table: "message_db_set",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_db_set_sender_id",
                table: "message_db_set",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_db_set_sent_at",
                table: "message_db_set",
                column: "sent_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_conversation");

            migrationBuilder.DropTable(
                name: "message_db_set");

            migrationBuilder.DropTable(
                name: "conversation_db_set");
        }
    }
}
