using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCancellationAndReturnAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "orders");

            migrationBuilder.CreateTable(
                name: "order_cancellations",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelled_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    refund_required = table.Column<bool>(type: "boolean", nullable: false),
                    inventory_rollback_required = table.Column<bool>(type: "boolean", nullable: false),
                    payment_rollback_required = table.Column<bool>(type: "boolean", nullable: false),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_cancellations", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_cancellations_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "return_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_return_orders_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "return_reasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_reasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "return_status_histories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    return_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<int>(type: "integer", nullable: false),
                    current_status = table.Column<int>(type: "integer", nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_return_status_histories_return_orders_return_order_id",
                        column: x => x.return_order_id,
                        principalTable: "return_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "return_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    return_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<long>(type: "bigint", nullable: false),
                    reason_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_return_items_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_return_items_return_orders_return_order_id",
                        column: x => x.return_order_id,
                        principalTable: "return_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_return_items_return_reasons_reason_id",
                        column: x => x.reason_id,
                        principalTable: "return_reasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_cancellations_tenant_id",
                table: "order_cancellations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_items_order_item_id",
                table: "return_items",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_items_reason_id",
                table: "return_items",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_items_return_order_id",
                table: "return_items",
                column: "return_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_items_tenant_id",
                table: "return_items",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_customer_id",
                table: "return_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_order_id",
                table: "return_orders",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_status",
                table: "return_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_return_orders_tenant_id",
                table: "return_orders",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_reasons_code",
                table: "return_reasons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_return_reasons_is_active",
                table: "return_reasons",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_return_reasons_tenant_id",
                table: "return_reasons",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_status_histories_return_order_id_changed_at",
                table: "return_status_histories",
                columns: new[] { "return_order_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_return_status_histories_tenant_id",
                table: "return_status_histories",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_cancellations");

            migrationBuilder.DropTable(
                name: "return_items");

            migrationBuilder.DropTable(
                name: "return_status_histories");

            migrationBuilder.DropTable(
                name: "return_reasons");

            migrationBuilder.DropTable(
                name: "return_orders");

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
