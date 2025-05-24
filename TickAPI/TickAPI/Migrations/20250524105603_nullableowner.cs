using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickAPI.Migrations
{
    /// <inheritdoc />
    public partial class nullableowner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Customers_OwnerId",
                table: "Tickets");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Customers_OwnerId",
                table: "Tickets",
                column: "OwnerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Customers_OwnerId",
                table: "Tickets");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Customers_OwnerId",
                table: "Tickets",
                column: "OwnerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
