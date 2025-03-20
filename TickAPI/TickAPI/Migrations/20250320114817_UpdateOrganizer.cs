using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Login",
                table: "Organizers");

            migrationBuilder.RenameColumn(
                name: "OrganizerName",
                table: "Organizers",
                newName: "DisplayName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "Organizers",
                newName: "OrganizerName");

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Organizers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
