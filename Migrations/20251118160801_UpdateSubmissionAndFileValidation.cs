using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubmissionAndFileValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Submissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Submissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Submissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_DocumentId",
                table: "Submissions",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Documents_DocumentId",
                table: "Submissions",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Documents_DocumentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_DocumentId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Submissions");
        }
    }
}
