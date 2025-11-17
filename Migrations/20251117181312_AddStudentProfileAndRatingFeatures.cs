using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfileAndRatingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRequirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentVersionId = table.Column<int>(type: "int", nullable: false),
                    AdvisorUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRatings_DocumentVersions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GPA = table.Column<double>(type: "float", nullable: true),
                    CompletedCredits = table.Column<int>(type: "int", nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MeetsPrerequisites = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseRequirementId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Grade = table.Column<double>(type: "float", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCourses_CourseRequirements_CourseRequirementId",
                        column: x => x.CourseRequirementId,
                        principalTable: "CourseRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRatings_DocumentVersionId",
                table: "DocumentRatings",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_CourseRequirementId",
                table: "StudentCourses",
                column: "CourseRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentRatings");

            migrationBuilder.DropTable(
                name: "StudentCourses");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "CourseRequirements");
        }
    }
}
