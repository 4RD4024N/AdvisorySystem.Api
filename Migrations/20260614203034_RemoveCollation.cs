using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCollation : Migration
    {
        // Azure SQL availability group'ta ALTER DATABASE desteklenmiyor — no-op
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) { }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
