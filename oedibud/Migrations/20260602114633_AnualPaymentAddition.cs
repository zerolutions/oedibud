using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace oedibud.Migrations
{
    /// <inheritdoc />
    public partial class AnualPaymentAddition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BruttoFactor",
                table: "Contracts",
                newName: "EmployerBruttoAddition");

            migrationBuilder.AddColumn<decimal>(
                name: "AnualPaymentAddition",
                table: "Contracts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnualPaymentAddition",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "EmployerBruttoAddition",
                table: "Contracts",
                newName: "BruttoFactor");
        }
    }
}
