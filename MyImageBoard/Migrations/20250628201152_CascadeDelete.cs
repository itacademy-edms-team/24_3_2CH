using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumProject.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Threads_ThreadId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Threads_ThreadId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Comments_CommentId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Threads_ThreadId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Comments_CommentId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Threads_ThreadId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizOptions_Quizzes_QuizId",
                table: "QuizOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizVotes_QuizOptions_QuizOptionId",
                table: "QuizVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Threads_ThreadId",
                table: "Quizzes");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Threads_ThreadId",
                table: "Comments",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Threads_ThreadId",
                table: "Complaints",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Comments_CommentId",
                table: "Likes",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Threads_ThreadId",
                table: "Likes",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Comments_CommentId",
                table: "MediaFiles",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Threads_ThreadId",
                table: "MediaFiles",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizOptions_Quizzes_QuizId",
                table: "QuizOptions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizVotes_QuizOptions_QuizOptionId",
                table: "QuizVotes",
                column: "QuizOptionId",
                principalTable: "QuizOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Threads_ThreadId",
                table: "Quizzes",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Threads_ThreadId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Threads_ThreadId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Comments_CommentId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Threads_ThreadId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Comments_CommentId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Threads_ThreadId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizOptions_Quizzes_QuizId",
                table: "QuizOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizVotes_QuizOptions_QuizOptionId",
                table: "QuizVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Threads_ThreadId",
                table: "Quizzes");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Threads_ThreadId",
                table: "Comments",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Comments_CommentId",
                table: "Complaints",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Threads_ThreadId",
                table: "Complaints",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Comments_CommentId",
                table: "Likes",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Threads_ThreadId",
                table: "Likes",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Comments_CommentId",
                table: "MediaFiles",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Threads_ThreadId",
                table: "MediaFiles",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizOptions_Quizzes_QuizId",
                table: "QuizOptions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizVotes_QuizOptions_QuizOptionId",
                table: "QuizVotes",
                column: "QuizOptionId",
                principalTable: "QuizOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Threads_ThreadId",
                table: "Quizzes",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id");
        }
    }
}
