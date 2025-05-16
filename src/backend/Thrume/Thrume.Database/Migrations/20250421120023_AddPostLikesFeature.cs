using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thrume.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPostLikesFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posts_accounts_author_id",
                table: "posts");

            migrationBuilder.AlterColumn<Guid>(
                name: "author_id",
                table: "posts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "account_post_likes",
                columns: table => new
                {
                    liked_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    liked_posts_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_post_likes", x => new { x.liked_by_id, x.liked_posts_id });
                    table.ForeignKey(
                        name: "fk_account_post_likes_accounts_liked_by_id",
                        column: x => x.liked_by_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_account_post_likes_posts_liked_posts_id",
                        column: x => x.liked_posts_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_post_likes_liked_posts_id",
                table: "account_post_likes",
                column: "liked_posts_id");

            migrationBuilder.AddForeignKey(
                name: "fk_posts_accounts_author_id",
                table: "posts",
                column: "author_id",
                principalTable: "accounts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posts_accounts_author_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "account_post_likes");

            migrationBuilder.AlterColumn<Guid>(
                name: "author_id",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_posts_accounts_author_id",
                table: "posts",
                column: "author_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
