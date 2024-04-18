using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTeam.Data.Migrations
{
    /// <inheritdoc />
    public partial class managearticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacultyId",
                table: "Article",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "Article",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Article_FacultyId",
                table: "Article",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Article_SemesterId",
                table: "Article",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Article_Faculty_FacultyId",
                table: "Article",
                column: "FacultyId",
                principalTable: "Faculty",
                principalColumn: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Article_Semester_SemesterId",
                table: "Article",
                column: "SemesterId",
                principalTable: "Semester",
                principalColumn: "SemesterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Article_Faculty_FacultyId",
                table: "Article");

            migrationBuilder.DropForeignKey(
                name: "FK_Article_Semester_SemesterId",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_FacultyId",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_SemesterId",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Article");
        }
    }
}
