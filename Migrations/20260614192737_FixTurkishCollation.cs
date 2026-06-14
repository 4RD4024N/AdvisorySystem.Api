using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvisorySystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixTurkishCollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DB zaten Turkish_CI_AS ile oluşturulduysa (önerilen) → no-op
            // Aksi hâlde tablolar varken ALTER DATABASE schema-bound index hatası verir.
            // Yeni Azure DB'yi Turkish_CI_AS collation ile oluşturun (bkz. README).
            migrationBuilder.Sql(@"
IF DATABASEPROPERTYEX(DB_NAME(), 'Collation') != 'Turkish_CI_AS'
BEGIN
    DECLARE @db NVARCHAR(128) = QUOTENAME(DB_NAME());
    EXEC(N'ALTER DATABASE ' + @db + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
    EXEC(N'ALTER DATABASE ' + @db + N' COLLATE Turkish_CI_AS');
    EXEC(N'ALTER DATABASE ' + @db + N' SET MULTI_USER');
END
", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF DATABASEPROPERTYEX(DB_NAME(), 'Collation') != 'SQL_Latin1_General_CP1_CI_AS'
BEGIN
    DECLARE @db NVARCHAR(128) = QUOTENAME(DB_NAME());
    EXEC(N'ALTER DATABASE ' + @db + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
    EXEC(N'ALTER DATABASE ' + @db + N' COLLATE SQL_Latin1_General_CP1_CI_AS');
    EXEC(N'ALTER DATABASE ' + @db + N' SET MULTI_USER');
END
", suppressTransaction: true);
        }
    }
}
