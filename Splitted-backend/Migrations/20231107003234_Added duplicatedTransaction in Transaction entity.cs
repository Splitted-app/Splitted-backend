using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Splitted_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedduplicatedTransactioninTransactionentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DuplicatedTransactionId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DuplicatedTransactionId",
                table: "Transactions",
                column: "DuplicatedTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Transactions_DuplicatedTransactionId",
                table: "Transactions",
                column: "DuplicatedTransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Transactions_DuplicatedTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DuplicatedTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DuplicatedTransactionId",
                table: "Transactions");
        }
    }
}
