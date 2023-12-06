using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Splitted_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedinCashfieldtoTransactionPayBackentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InCash",
                table: "TransactionPayBacks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InCash",
                table: "TransactionPayBacks");
        }
    }
}
