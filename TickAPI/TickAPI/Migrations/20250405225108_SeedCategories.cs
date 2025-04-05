using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TickAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "Categories",
                newName: "Name");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("4421327a-4bc8-4706-bec0-666f78ed0c69"), "Workshops" },
                    { new Guid("4a086d9e-59de-4fd1-a1b2-bd9b5eec797c"), "Theatre" },
                    { new Guid("5f8dbe65-30be-453f-8f22-191a11b2977b"), "Comedy" },
                    { new Guid("de89dd76-3b29-43e1-8f4b-5278b1b8bde2"), "Sports" },
                    { new Guid("ea58370b-2a17-4770-abea-66399ad69fb8"), "Conferences" },
                    { new Guid("ec3daf69-baa9-4fcd-a674-c09884a57272"), "Music" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4421327a-4bc8-4706-bec0-666f78ed0c69"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4a086d9e-59de-4fd1-a1b2-bd9b5eec797c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5f8dbe65-30be-453f-8f22-191a11b2977b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("de89dd76-3b29-43e1-8f4b-5278b1b8bde2"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ea58370b-2a17-4770-abea-66399ad69fb8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ec3daf69-baa9-4fcd-a674-c09884a57272"));

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "CategoryName");
        }
    }
}
