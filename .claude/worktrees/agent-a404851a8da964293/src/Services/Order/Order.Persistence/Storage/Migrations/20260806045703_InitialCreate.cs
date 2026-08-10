using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer_name = table.Column<string>(type: "text", nullable: false),
                    topic = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    headers_json = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_retry_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer_name = table.Column<string>(type: "text", nullable: false),
                    topic = table.Column<string>(type: "text", nullable: false),
                    retry_number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: true),
                    @operator = table.Column<string>(name: "operator", type: "text", nullable: true),
                    result = table.Column<int>(type: "integer", nullable: false),
                    exception = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_retry_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    cancellation_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    shipping_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_method = table.Column<int>(type: "integer", nullable: false),
                    tax_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_applied_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    topic = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: true),
                    actor_type = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_catalogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    variation_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_catalogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saga_execution_records",
                columns: table => new
                {
                    saga_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    saga_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    completed_steps_json = table.Column<string>(type: "jsonb", nullable: false),
                    context_data_json = table.Column<string>(type: "jsonb", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saga_execution_records", x => x.saga_id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_no = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    variation_name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    product_promotion_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bundle_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    gift_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    coupon_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    manual_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    final_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_owners",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    owner_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    owner_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    owner_phone_search = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_phone_reverse = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_owners", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_owners_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_shippings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    receiver_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    original_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    final_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    shipping_method = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    shipped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arrived_at_warehouse_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    in_transit_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_shippings", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_shippings_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_discounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<long>(type: "bigint", nullable: true),
                    order_shipping_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    source_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rule_method = table.Column<int>(type: "integer", nullable: false),
                    rule_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    rule_maximum_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    rule_minimum_spend = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    applied_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_discounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_discounts_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_discounts_order_shipping_order_shipping_id",
                        column: x => x.order_shipping_id,
                        principalTable: "order_shippings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_discounts_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_inbox_message_consumer_unique",
                table: "inbox_messages",
                columns: new[] { "message_id", "consumer_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_inbox_processed_at",
                table: "inbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_inbox_status_next_retry_at",
                table: "inbox_messages",
                columns: new[] { "status", "next_retry_at" });

            migrationBuilder.CreateIndex(
                name: "ix_order_discounts_order_id",
                table: "order_discounts",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_discounts_order_item_id",
                table: "order_discounts",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_discounts_order_shipping_id",
                table: "order_discounts",
                column: "order_shipping_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_discounts_tenant_id",
                table: "order_discounts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_tenant_id",
                table: "order_items",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_idempotency_key",
                table: "order_owners",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_owner_id",
                table: "order_owners",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_owner_id_idempotency_key",
                table: "order_owners",
                columns: new[] { "owner_id", "idempotency_key" },
                unique: true,
                filter: "\"idempotency_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_owner_name",
                table: "order_owners",
                column: "owner_name");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_owner_phone_reverse",
                table: "order_owners",
                column: "owner_phone_reverse");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_owner_phone_search",
                table: "order_owners",
                column: "owner_phone_search");

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_tenant_id",
                table: "order_owners",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_shippings_idempotency_key",
                table: "order_shippings",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_order_shippings_order_id",
                table: "order_shippings",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_shippings_tenant_id",
                table: "order_shippings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_orders_idempotency_key",
                table: "orders",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_created_at",
                table: "orders",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id",
                table: "orders",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_product_catalogs_product_id_variation_id",
                table: "product_catalogs",
                columns: new[] { "product_id", "variation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_catalogs_tenant_id",
                table: "product_catalogs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_saga_name_started_at",
                table: "saga_execution_records",
                columns: new[] { "saga_name", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_state_started_at",
                table: "saga_execution_records",
                columns: new[] { "state", "started_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "inbox_retry_histories");

            migrationBuilder.DropTable(
                name: "order_discounts");

            migrationBuilder.DropTable(
                name: "order_owners");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "product_catalogs");

            migrationBuilder.DropTable(
                name: "saga_execution_records");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_shippings");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
