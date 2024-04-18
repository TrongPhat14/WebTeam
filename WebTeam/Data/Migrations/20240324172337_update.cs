using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTeam.Data.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submission_FeedBack_FeedBackID",
                table: "Submission");

            migrationBuilder.DropIndex(
                name: "IX_Submission_FeedBackID",
                table: "Submission");

            migrationBuilder.DropColumn(
                name: "FeedBackID",
                table: "Submission");

            migrationBuilder.AlterColumn<string>(
                name: "FileSubName",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CommentSubDateF",
                table: "Submission",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFeedbackUpdateTime",
                table: "Submission",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SendF",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentSubDateF",
                table: "Submission");

            migrationBuilder.DropColumn(
                name: "LastFeedbackUpdateTime",
                table: "Submission");

            migrationBuilder.DropColumn(
                name: "SendF",
                table: "Submission");

            migrationBuilder.DropColumn(
                name: "image",
                table: "Submission");

            migrationBuilder.DropColumn(
                name: "Send",
                table: "FeedBack");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "FeedBack");

            migrationBuilder.AlterColumn<string>(
                name: "FileSubName",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedBackID",
                table: "Submission",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submission_FeedBackID",
                table: "Submission",
                column: "FeedBackID");

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_FeedBack_FeedBackID",
                table: "Submission",
                column: "FeedBackID",
                principalTable: "FeedBack",
                principalColumn: "FeedBackID");
        }
    }
}
