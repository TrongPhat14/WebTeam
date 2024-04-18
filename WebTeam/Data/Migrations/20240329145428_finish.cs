using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTeam.Data.Migrations
{
    /// <inheritdoc />
    public partial class finish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedBack_Submission_SubmissionID",
                table: "FeedBack");

            migrationBuilder.DropTable(
                name: "Submission");

            migrationBuilder.DropColumn(
                name: "Send",
                table: "FeedBack");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "FeedBack");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "SubmissionID",
                table: "FeedBack",
                newName: "ArticleId");

            migrationBuilder.RenameIndex(
                name: "IX_FeedBack_SubmissionID",
                table: "FeedBack",
                newName: "IX_FeedBack_ArticleId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArticleDate",
                table: "Article",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Article",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Isbool",
                table: "Article",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "submissionDate",
                table: "Article",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBack_Article_ArticleId",
                table: "FeedBack",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedBack_Article_ArticleId",
                table: "FeedBack");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "Isbool",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "submissionDate",
                table: "Article");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "FeedBack",
                newName: "SubmissionID");

            migrationBuilder.RenameIndex(
                name: "IX_FeedBack_ArticleId",
                table: "FeedBack",
                newName: "IX_FeedBack_SubmissionID");

            migrationBuilder.AddColumn<string>(
                name: "Send",
                table: "FeedBack",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "FeedBack",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AspNetUsers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArticleDate",
                table: "Article",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Submission",
                columns: table => new
                {
                    SubmissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CommentSubContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentSubDateF = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileSubName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastFeedbackUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SendF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissonName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submission", x => x.SubmissionID);
                    table.ForeignKey(
                        name: "FK_Submission_AspNetUsers_StudentID",
                        column: x => x.StudentID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submission_StudentID",
                table: "Submission",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBack_Submission_SubmissionID",
                table: "FeedBack",
                column: "SubmissionID",
                principalTable: "Submission",
                principalColumn: "SubmissionID");
        }
    }
}
