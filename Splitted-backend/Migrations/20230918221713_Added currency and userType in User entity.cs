using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Splitted_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedcurrencyanduserTypeinUserentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mail",
                table: "User",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "User",
                newName: "Mail");
        }
    }
}
