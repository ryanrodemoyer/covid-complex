using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace web.Migrations
{
    public partial class InitialMigrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastUpdatedWhen = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    StateId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StateName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "Counties",
                columns: table => new
                {
                    CountyId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StateId = table.Column<int>(nullable: false),
                    CountyName = table.Column<string>(nullable: true),
                    Fips = table.Column<int>(nullable: false),
                    Population = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counties", x => x.CountyId);
                    table.ForeignKey(
                        name: "FK_Counties_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CovidDays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountyId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Cases = table.Column<int>(nullable: false),
                    Deaths = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CovidDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CovidDays_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "CountyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CovidDaysStaging",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    Cases = table.Column<int>(nullable: false),
                    Deaths = table.Column<int>(nullable: false),
                    CountyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CovidDaysStaging", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CovidDaysStaging_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "CountyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Counties_StateId",
                table: "Counties",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CovidDays_CountyId_Date",
                table: "CovidDays",
                columns: new[] { "CountyId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CovidDaysStaging_CountyId",
                table: "CovidDaysStaging",
                column: "CountyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CovidDays");

            migrationBuilder.DropTable(
                name: "CovidDaysStaging");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "Counties");

            migrationBuilder.DropTable(
                name: "States");
        }
    }
}
