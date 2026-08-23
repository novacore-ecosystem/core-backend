using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Content.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddContentVersionLanguageAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_content_localizations_content_id_culture",
                table: "content_localizations");

            migrationBuilder.DropIndex(
                name: "ix_content_localizations_version_id",
                table: "content_localizations");

            migrationBuilder.DropColumn(
                name: "body",
                table: "content_versions");

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "content_versions");

            migrationBuilder.DropColumn(
                name: "summary",
                table: "content_versions");

            migrationBuilder.DropColumn(
                name: "title",
                table: "content_versions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "content_localizations");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "contents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "contents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "content_localizations",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "metadata",
                table: "content_localizations",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "summary",
                table: "content_localizations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "content_localizations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_contents_is_deleted",
                table: "contents",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_content_id_culture",
                table: "content_localizations",
                columns: new[] { "content_id", "culture" });

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_version_id_culture",
                table: "content_localizations",
                columns: new[] { "version_id", "culture" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_contents_is_deleted",
                table: "contents");

            migrationBuilder.DropIndex(
                name: "ix_content_localizations_content_id_culture",
                table: "content_localizations");

            migrationBuilder.DropIndex(
                name: "ix_content_localizations_version_id_culture",
                table: "content_localizations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "body",
                table: "content_localizations");

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "content_localizations");

            migrationBuilder.DropColumn(
                name: "summary",
                table: "content_localizations");

            migrationBuilder.DropColumn(
                name: "title",
                table: "content_localizations");

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "content_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "metadata",
                table: "content_versions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "summary",
                table: "content_versions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "content_versions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "status",
                table: "content_localizations",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_content_id_culture",
                table: "content_localizations",
                columns: new[] { "content_id", "culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_version_id",
                table: "content_localizations",
                column: "version_id");
        }
    }
}
