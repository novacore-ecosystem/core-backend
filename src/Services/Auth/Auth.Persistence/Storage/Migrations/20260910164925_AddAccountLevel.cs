using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Auth.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "level",
                table: "users");
        }
    }
}
