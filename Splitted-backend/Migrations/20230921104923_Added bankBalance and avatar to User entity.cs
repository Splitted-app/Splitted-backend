using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Splitted_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedbankBalanceandavatartoUserentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "AvatarImage",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BankBalance",
                table: "User",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarImage",
                table: "User");

            migrationBuilder.DropColumn(
                name: "BankBalance",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
