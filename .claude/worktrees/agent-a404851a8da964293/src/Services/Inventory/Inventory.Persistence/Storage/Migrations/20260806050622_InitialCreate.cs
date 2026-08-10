using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NovaCore.Inventory.Persistence.Storage.Migrations
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
                name: "warehouses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, defaultValue: ""),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "UTC"),
                    supports_receiving = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_shipping = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_reservation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_transfer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_picking = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_returns = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_negative_stock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_warehouses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_counts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    count_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, defaultValue: ""),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_counts", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_counts_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    reason = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    source_warehouse_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_warehouse_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, defaultValue: ""),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_documents_warehouses_destination_warehouse_id",
                        column: x => x.destination_warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_documents_warehouses_source_warehouse_id",
                        column: x => x.source_warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    on_hand_quantity = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false),
                    incoming_quantity = table.Column<int>(type: "integer", nullable: false),
                    outgoing_quantity = table.Column<int>(type: "integer", nullable: false),
                    damaged_quantity = table.Column<int>(type: "integer", nullable: false),
                    safety_stock = table.Column<int>(type: "integer", nullable: false),
                    reorder_point = table.Column<int>(type: "integer", nullable: false),
                    maximum_stock = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_stocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_stocks_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_zones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, defaultValue: ""),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    picking_strategy = table.Column<short>(type: "smallint", nullable: false),
                    allow_mixed_lot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_warehouse_zones", x => x.id);
                    table.ForeignKey(
                        name: "fk_warehouse_zones_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_count_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventory_count_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expected_quantity = table.Column<int>(type: "integer", nullable: false),
                    actual_quantity = table.Column<int>(type: "integer", nullable: true),
                    difference_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, defaultValue: ""),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_count_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_count_items_inventory_counts_inventory_count_id",
                        column: x => x.inventory_count_id,
                        principalTable: "inventory_counts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_count_items_inventory_stocks_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_document_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventory_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_of_measure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    inventory_lot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inventory_serial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, defaultValue: ""),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_document_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_document_items_inventory_documents_inventory_docu",
                        column: x => x.inventory_document_id,
                        principalTable: "inventory_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_document_items_inventory_stocks_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_lots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    manufacture_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    supplier_lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: ""),
                    country_of_origin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: ""),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_lots", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_lots_inventory_stocks_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<short>(type: "smallint", nullable: true),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValue: ""),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, defaultValue: ""),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_reservations_inventory_stocks_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_reservations_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_serials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    inventory_reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inventory_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_serials", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_serials_inventory_documents_inventory_document_id",
                        column: x => x.inventory_document_id,
                        principalTable: "inventory_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_serials_inventory_reservations_inventory_reservat",
                        column: x => x.inventory_reservation_id,
                        principalTable: "inventory_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_inventory_serials_inventory_stocks_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inventory_reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    before_on_hand_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    after_on_hand_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    before_reserved_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    after_reserved_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_transactions_inventory_documents_inventory_docume",
                        column: x => x.inventory_document_id,
                        principalTable: "inventory_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_transactions_inventory_reservations_inventory_res",
                        column: x => x.inventory_reservation_id,
                        principalTable: "inventory_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_transactions_inventory_stocks_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_transactions_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "ix_inventory_count_items_inventory_count_id",
                table: "inventory_count_items",
                column: "inventory_count_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_count_items_inventory_id",
                table: "inventory_count_items",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_count_items_tenant_id",
                table: "inventory_count_items",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_counts_count_date",
                table: "inventory_counts",
                column: "count_date");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_counts_number",
                table: "inventory_counts",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_counts_status",
                table: "inventory_counts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_counts_tenant_id",
                table: "inventory_counts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_counts_warehouse_id",
                table: "inventory_counts",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_document_items_inventory_document_id",
                table: "inventory_document_items",
                column: "inventory_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_document_items_inventory_id",
                table: "inventory_document_items",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_document_items_tenant_id",
                table: "inventory_document_items",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_document_items_variant_id",
                table: "inventory_document_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_documents_destination_warehouse_id",
                table: "inventory_documents",
                column: "destination_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_documents_number",
                table: "inventory_documents",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_documents_source_warehouse_id",
                table: "inventory_documents",
                column: "source_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_documents_status",
                table: "inventory_documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_documents_tenant_id",
                table: "inventory_documents",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_documents_type",
                table: "inventory_documents",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_expired_date",
                table: "inventory_lots",
                column: "expired_date");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_inventory_id",
                table: "inventory_lots",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_lot_number",
                table: "inventory_lots",
                column: "lot_number");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_status",
                table: "inventory_lots",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_tenant_id",
                table: "inventory_lots",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_expired_at",
                table: "inventory_reservations",
                column: "expired_at");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_inventory_id",
                table: "inventory_reservations",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_number",
                table: "inventory_reservations",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_reference_type_reference_id",
                table: "inventory_reservations",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_status",
                table: "inventory_reservations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_tenant_id",
                table: "inventory_reservations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_warehouse_id",
                table: "inventory_reservations",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_serials_inventory_document_id",
                table: "inventory_serials",
                column: "inventory_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_serials_inventory_id",
                table: "inventory_serials",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_serials_inventory_reservation_id",
                table: "inventory_serials",
                column: "inventory_reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_serials_serial_number",
                table: "inventory_serials",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_serials_status",
                table: "inventory_serials",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_serials_tenant_id",
                table: "inventory_serials",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_stocks_product_id",
                table: "inventory_stocks",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_stocks_status",
                table: "inventory_stocks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_stocks_tenant_id",
                table: "inventory_stocks",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_stocks_variant_id_warehouse_id",
                table: "inventory_stocks",
                columns: new[] { "variant_id", "warehouse_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_stocks_warehouse_id",
                table: "inventory_stocks",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_inventory_document_id",
                table: "inventory_transactions",
                column: "inventory_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_inventory_id_created_at",
                table: "inventory_transactions",
                columns: new[] { "inventory_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_inventory_reservation_id",
                table: "inventory_transactions",
                column: "inventory_reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_product_id",
                table: "inventory_transactions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_tenant_id",
                table: "inventory_transactions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_type",
                table: "inventory_transactions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_warehouse_id",
                table: "inventory_transactions",
                column: "warehouse_id");

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
                name: "ix_warehouse_zones_status",
                table: "warehouse_zones",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_warehouse_zones_tenant_id",
                table: "warehouse_zones",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_warehouse_zones_type",
                table: "warehouse_zones",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_warehouse_zones_warehouse_id_code",
                table: "warehouse_zones",
                columns: new[] { "warehouse_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_warehouses_code",
                table: "warehouses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_warehouses_status",
                table: "warehouses",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_warehouses_tenant_id",
                table: "warehouses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_warehouses_type",
                table: "warehouses",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "inbox_retry_histories");

            migrationBuilder.DropTable(
                name: "inventory_count_items");

            migrationBuilder.DropTable(
                name: "inventory_document_items");

            migrationBuilder.DropTable(
                name: "inventory_lots");

            migrationBuilder.DropTable(
                name: "inventory_serials");

            migrationBuilder.DropTable(
                name: "inventory_transactions");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "warehouse_zones");

            migrationBuilder.DropTable(
                name: "inventory_counts");

            migrationBuilder.DropTable(
                name: "inventory_documents");

            migrationBuilder.DropTable(
                name: "inventory_reservations");

            migrationBuilder.DropTable(
                name: "inventory_stocks");

            migrationBuilder.DropTable(
                name: "warehouses");
        }
    }
}
