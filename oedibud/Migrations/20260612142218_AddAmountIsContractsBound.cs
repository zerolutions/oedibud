using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace oedibud.Migrations
{
    /// <inheritdoc />
    public partial class AddAmountIsContractsBound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AmountIsContractsBound",
                table: "Payments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountIsContractsBound",
                table: "Payments");
        }
    }
}
