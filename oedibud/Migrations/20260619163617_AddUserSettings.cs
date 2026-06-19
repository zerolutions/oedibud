using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace oedibud.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartMonth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QuarterView = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpandedEmployeeIds = table.Column<string>(type: "TEXT", nullable: false),
                    ExpandedProjectIds = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
