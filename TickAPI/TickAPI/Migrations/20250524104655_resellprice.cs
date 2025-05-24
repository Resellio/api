using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickAPI.Migrations
{
    /// <inheritdoc />
    public partial class resellprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ResellPrice",
                table: "Tickets",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResellPrice",
                table: "Tickets");
        }
    }
}
