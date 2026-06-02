using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace oedibud.Migrations
{
    /// <inheritdoc />
    public partial class ContractShareSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharePercent",
                table: "ContractPayments");

            migrationBuilder.AddColumn<decimal>(
                name: "ContractShare",
                table: "ContractPayments",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractShare",
                table: "ContractPayments");

            migrationBuilder.AddColumn<int>(
                name: "SharePercent",
                table: "ContractPayments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
