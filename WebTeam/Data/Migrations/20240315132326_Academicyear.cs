using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTeam.Data.Migrations
{
    /// <inheritdoc />
    public partial class Academicyear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Submission");

            migrationBuilder.AddColumn<string>(
                name: "SubmissonName",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicYear",
                columns: table => new
                {
                    AcademicYearID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYears = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYear", x => x.AcademicYearID);
                });

            migrationBuilder.CreateTable(
                name: "Semester",
                columns: table => new
                {
                    SemesterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dl1 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DL2 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FacultyID = table.Column<int>(type: "int", nullable: true),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester", x => x.SemesterID);
                    table.ForeignKey(
                        name: "FK_Semester_AcademicYear_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "AcademicYear",
                        principalColumn: "AcademicYearID");
                    table.ForeignKey(
                        name: "FK_Semester_Faculty_FacultyID",
                        column: x => x.FacultyID,
                        principalTable: "Faculty",
                        principalColumn: "FacultyId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Semester_AcademicYearID",
                table: "Semester",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_FacultyID",
                table: "Semester",
                column: "FacultyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Semester");

            migrationBuilder.DropTable(
                name: "AcademicYear");

            migrationBuilder.DropColumn(
                name: "SubmissonName",
                table: "Submission");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
