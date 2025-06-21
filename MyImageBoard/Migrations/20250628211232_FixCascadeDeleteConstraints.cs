using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumProject.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
