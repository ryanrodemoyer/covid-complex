using Microsoft.EntityFrameworkCore.Migrations;

namespace web.Migrations
{
    public partial class SiteSettingsUsCountiesSha : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "USCountiesSha",
                table: "SiteSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "USCountiesSha",
                table: "SiteSettings");
        }
    }
}
