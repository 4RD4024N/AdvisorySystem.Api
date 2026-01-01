using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentCourseRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_CourseRequirements_CourseRequirementId",
                table: "StudentCourses");

            migrationBuilder.RenameColumn(
                name: "CourseRequirementId",
                table: "StudentCourses",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourses_CourseRequirementId",
                table: "StudentCourses",
                newName: "IX_StudentCourses_CourseId");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrolledAt",
                table: "StudentCourses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LetterGrade",
                table: "StudentCourses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "StudentCourses",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_Courses_CourseId",
                table: "StudentCourses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_Courses_CourseId",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "EnrolledAt",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "LetterGrade",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "StudentCourses");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "StudentCourses",
                newName: "CourseRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourses_CourseId",
                table: "StudentCourses",
                newName: "IX_StudentCourses_CourseRequirementId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_CourseRequirements_CourseRequirementId",
                table: "StudentCourses",
                column: "CourseRequirementId",
                principalTable: "CourseRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
