using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace oedibud.Migrations
{
    /// <inheritdoc />
    public partial class GroupToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BruttoFactor",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ExperienceMonth",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Employees");

            migrationBuilder.AlterColumn<decimal>(
                name: "Fte",
                table: "Contracts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL");

            migrationBuilder.AddColumn<decimal>(
                name: "BruttoFactor",
                table: "Contracts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMonth",
                table: "Contracts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Group",
                table: "Contracts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Contracts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BruttoFactor",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ExperienceMonth",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Contracts");

            migrationBuilder.AddColumn<decimal>(
                name: "BruttoFactor",
                table: "Employees",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMonth",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Group",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<float>(
                name: "Fte",
                table: "Contracts",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");
        }
    }
}
