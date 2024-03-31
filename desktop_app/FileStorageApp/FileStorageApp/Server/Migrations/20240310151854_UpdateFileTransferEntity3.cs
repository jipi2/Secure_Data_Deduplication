using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFileTransferEntity3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileTransfers_Users_RecieverId",
                table: "FileTransfers");

            migrationBuilder.DropIndex(
                name: "IX_FileTransfers_RecieverId",
                table: "FileTransfers");

            migrationBuilder.DropColumn(
                name: "RecieverId",
                table: "FileTransfers");

            migrationBuilder.RenameColumn(
                name: "base64Iv",
                table: "FileTransfers",
                newName: "base64EncIv");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "base64EncIv",
                table: "FileTransfers",
                newName: "base64Iv");

            migrationBuilder.AddColumn<int>(
                name: "RecieverId",
                table: "FileTransfers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileTransfers_RecieverId",
                table: "FileTransfers",
                column: "RecieverId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileTransfers_Users_RecieverId",
                table: "FileTransfers",
                column: "RecieverId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
