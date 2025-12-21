using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAdvisorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_DocumentVersions_DocumentVersionId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_DocumentVersions_DocumentId_VersionNo",
                table: "DocumentVersions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_DocumentVersionId",
                table: "Comments");

            migrationBuilder.AddColumn<string>(
                name: "AdvisorId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId",
                table: "DocumentVersions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AdvisorId",
                table: "AspNetUsers",
                column: "AdvisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AdvisorId",
                table: "AspNetUsers",
                column: "AdvisorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AdvisorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_DocumentVersions_DocumentId",
                table: "DocumentVersions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AdvisorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AdvisorId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId_VersionNo",
                table: "DocumentVersions",
                columns: new[] { "DocumentId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DocumentVersionId",
                table: "Comments",
                column: "DocumentVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_DocumentVersions_DocumentVersionId",
                table: "Comments",
                column: "DocumentVersionId",
                principalTable: "DocumentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
