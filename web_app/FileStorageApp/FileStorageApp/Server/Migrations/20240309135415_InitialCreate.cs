using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilesMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlobLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerDHPrivate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerDHPublic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    G = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    P = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SymKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Base64RSAPublicKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Base64RSAEncPrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Position_1 = table.Column<int>(type: "int", nullable: false),
                    Position_2 = table.Column<int>(type: "int", nullable: false),
                    Position_3 = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    wasUsed = table.Column<bool>(type: "bit", nullable: false),
                    FileMetadataId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resps_FilesMetadata_FileMetadataId",
                        column: x => x.FileMetadataId,
                        principalTable: "FilesMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Iv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFiles_FilesMetadata_FileId",
                        column: x => x.FileId,
                        principalTable: "FilesMetadata",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resps_FileMetadataId",
                table: "Resps",
                column: "FileMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFiles_FileId",
                table: "UserFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFiles_UserId",
                table: "UserFiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UsersId",
                table: "UserRoles",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resps");

            migrationBuilder.DropTable(
                name: "UserFiles");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "FilesMetadata");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
