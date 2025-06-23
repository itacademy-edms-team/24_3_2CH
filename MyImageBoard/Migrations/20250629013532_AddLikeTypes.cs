using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumProject.Migrations
{
    /// <inheritdoc />
    public partial class AddLikeTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Likes_UserFingerprintId_CommentId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_UserFingerprintId_ThreadId",
                table: "Likes");

            migrationBuilder.AddColumn<int>(
                name: "LikeTypeId",
                table: "Likes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LikeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_LikeTypeId",
                table: "Likes",
                column: "LikeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserFingerprintId_CommentId_LikeTypeId",
                table: "Likes",
                columns: new[] { "UserFingerprintId", "CommentId", "LikeTypeId" },
                unique: true,
                filter: "[CommentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserFingerprintId_ThreadId_LikeTypeId",
                table: "Likes",
                columns: new[] { "UserFingerprintId", "ThreadId", "LikeTypeId" },
                unique: true,
                filter: "[ThreadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_LikeTypes_LikeTypeId",
                table: "Likes",
                column: "LikeTypeId",
                principalTable: "LikeTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_LikeTypes_LikeTypeId",
                table: "Likes");

            migrationBuilder.DropTable(
                name: "LikeTypes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_LikeTypeId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_UserFingerprintId_CommentId_LikeTypeId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_UserFingerprintId_ThreadId_LikeTypeId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "LikeTypeId",
                table: "Likes");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserFingerprintId_CommentId",
                table: "Likes",
                columns: new[] { "UserFingerprintId", "CommentId" },
                unique: true,
                filter: "[CommentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserFingerprintId_ThreadId",
                table: "Likes",
                columns: new[] { "UserFingerprintId", "ThreadId" },
                unique: true,
                filter: "[ThreadId] IS NOT NULL");
        }
    }
}
