using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanillaRoles.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAndGroupName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Links",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "McGroup",
                table: "Links",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "McGroup",
                table: "Links");
        }
    }
}
