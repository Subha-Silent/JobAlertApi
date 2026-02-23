using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAlertApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_Users_UserId",
                table: "refresh");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh",
                table: "refresh");

            migrationBuilder.RenameTable(
                name: "refresh",
                newName: "RefreshTokens");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_UserId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "refresh",
                newName: "IX_refresh_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh",
                table: "refresh",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_Users_UserId",
                table: "refresh",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
