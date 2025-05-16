using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thrume.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNestedCommentFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comment_db_set",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment_db_set", x => x.id);
                    table.ForeignKey(
                        name: "fk_comment_db_set_accounts_author_id",
                        column: x => x.author_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comment_db_set_comment_db_set_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "comment_db_set",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_comment_db_set_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comment_db_set_author_id",
                table: "comment_db_set",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_db_set_parent_comment_id",
                table: "comment_db_set",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_db_set_post_id",
                table: "comment_db_set",
                column: "post_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comment_db_set");
        }
    }
}
