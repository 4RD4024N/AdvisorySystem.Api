using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSemesterFromStudentCourseSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "StudentCourseSections");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "StudentCourseSections");

            migrationBuilder.DropColumn(
                name: "LetterGrade",
                table: "StudentCourseSections");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "StudentCourseSections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "StudentCourseSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Grade",
                table: "StudentCourseSections",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LetterGrade",
                table: "StudentCourseSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "StudentCourseSections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
