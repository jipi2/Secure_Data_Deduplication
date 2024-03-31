using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class FileTransferEntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecieverId",
                table: "FileTransfers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileTransfers_RecieverId",
                table: "FileTransfers",
                column: "RecieverId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTransfers_SenderId",
                table: "FileTransfers",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileTransfers_Users_RecieverId",
                table: "FileTransfers",
                column: "RecieverId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileTransfers_Users_SenderId",
                table: "FileTransfers",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileTransfers_Users_RecieverId",
                table: "FileTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_FileTransfers_Users_SenderId",
                table: "FileTransfers");

            migrationBuilder.DropIndex(
                name: "IX_FileTransfers_RecieverId",
                table: "FileTransfers");

            migrationBuilder.DropIndex(
                name: "IX_FileTransfers_SenderId",
                table: "FileTransfers");

            migrationBuilder.DropColumn(
                name: "RecieverId",
                table: "FileTransfers");
        }
    }
}
