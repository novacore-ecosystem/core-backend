using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Product.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages");
        }
    }
}
