using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_tag_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_tag_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_tags",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_tags", x => new { x.order_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_order_tags_order_tag_definitions_tag_id",
                        column: x => x.tag_id,
                        principalTable: "order_tag_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_tags_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_tag_definitions_code",
                table: "order_tag_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_tag_definitions_is_active",
                table: "order_tag_definitions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_order_tag_definitions_tenant_id",
                table: "order_tag_definitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_tags_tag_id",
                table: "order_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_tags_tenant_id",
                table: "order_tags",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_tags");

            migrationBuilder.DropTable(
                name: "order_tag_definitions");
        }
    }
}
