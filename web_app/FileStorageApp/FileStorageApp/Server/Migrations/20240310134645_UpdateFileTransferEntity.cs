using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFileTransferEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "base64Iv",
                table: "FileTransfers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "base64Iv",
                table: "FileTransfers");
        }
    }
}
