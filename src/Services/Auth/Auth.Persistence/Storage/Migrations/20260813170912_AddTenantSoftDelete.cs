using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Auth.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_is_deleted",
                table: "tenants",
                column: "is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tenants_is_deleted",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tenants");
        }
    }
}
