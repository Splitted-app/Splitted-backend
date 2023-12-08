using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Splitted_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedowingUserandowedUserinTransactionPayBackentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionPayBacks_Users_UserId",
                table: "TransactionPayBacks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TransactionPayBacks",
                newName: "OwingUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionPayBacks_UserId",
                table: "TransactionPayBacks",
                newName: "IX_TransactionPayBacks_OwingUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "OwedUserId",
                table: "TransactionPayBacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayBacks_OwedUserId",
                table: "TransactionPayBacks",
                column: "OwedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionPayBacks_Users_OwedUserId",
                table: "TransactionPayBacks",
                column: "OwedUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionPayBacks_Users_OwingUserId",
                table: "TransactionPayBacks",
                column: "OwingUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionPayBacks_Users_OwedUserId",
                table: "TransactionPayBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionPayBacks_Users_OwingUserId",
                table: "TransactionPayBacks");

            migrationBuilder.DropIndex(
                name: "IX_TransactionPayBacks_OwedUserId",
                table: "TransactionPayBacks");

            migrationBuilder.DropColumn(
                name: "OwedUserId",
                table: "TransactionPayBacks");

            migrationBuilder.RenameColumn(
                name: "OwingUserId",
                table: "TransactionPayBacks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionPayBacks_OwingUserId",
                table: "TransactionPayBacks",
                newName: "IX_TransactionPayBacks_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionPayBacks_Users_UserId",
                table: "TransactionPayBacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
