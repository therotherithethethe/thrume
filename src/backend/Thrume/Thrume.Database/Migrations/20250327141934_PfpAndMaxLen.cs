using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thrume.Database.Migrations
{
    /// <inheritdoc />
    public partial class PfpAndMaxLen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "posts",
                type: "character varying(280)",
                maxLength: 280,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "picture_url",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "picture_url",
                table: "accounts");

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(280)",
                oldMaxLength: 280);
        }
    }
}
