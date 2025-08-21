using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReshamBazaar.Api.Migrations
{
    /// <inheritdoc />
    public partial class ssm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Add IsBlocked only if it doesn't already exist (handles manual SQL adds)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.AspNetUsers', 'IsBlocked') IS NULL
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [IsBlocked] bit NOT NULL CONSTRAINT DF_AspNetUsers_IsBlocked DEFAULT(0);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            // Drop IsBlocked only if it exists
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.AspNetUsers', 'IsBlocked') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    DROP CONSTRAINT DF_AspNetUsers_IsBlocked;
    ALTER TABLE [dbo].[AspNetUsers]
    DROP COLUMN [IsBlocked];
END
");
        }
    }
}
