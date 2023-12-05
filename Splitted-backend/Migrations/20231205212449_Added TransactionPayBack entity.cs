using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Splitted_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedTransactionPayBackentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionPayBacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    TransactionPayBackStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayBackTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPayBacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionPayBacks_Transactions_OriginalTransactionId",
                        column: x => x.OriginalTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionPayBacks_Transactions_PayBackTransactionId",
                        column: x => x.PayBackTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionPayBacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayBacks_OriginalTransactionId",
                table: "TransactionPayBacks",
                column: "OriginalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayBacks_PayBackTransactionId",
                table: "TransactionPayBacks",
                column: "PayBackTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayBacks_UserId",
                table: "TransactionPayBacks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionPayBacks");
        }
    }
}
