using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NovaCore.Shipping.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carrier_integrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipping_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integration_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    api_key_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    secret_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    webhook_secret_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carrier_integrations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transportation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    receiver_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cod_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    cod_collected = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deliveries", x => x.id);
                });

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
                name: "pickups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_type = table.Column<short>(type: "smallint", nullable: false),
                    address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pickups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "return_shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    returned_shipment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requested_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_shipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shipment_type = table.Column<short>(type: "smallint", nullable: false),
                    source_type = table.Column<short>(type: "smallint", nullable: false),
                    source_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    sender_address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sender_address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sender_address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sender_address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sender_address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    sender_address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sender_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sender_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    receiver_address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    receiver_address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver_address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver_address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver_address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    receiver_address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    receiver_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    receiver_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    declared_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    requested_pickup_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expected_delivery_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipping_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    verification_status = table.Column<short>(type: "smallint", nullable: false),
                    verified_address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipping_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipping_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    provider_type = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipping_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transportation_cost_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rule_type = table.Column<short>(type: "smallint", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    base_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    max_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_cost_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transportation_people",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_people", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transportation_vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    capacity_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    capacity_m3 = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transportations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transportation_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_no = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    cost_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    distance_km = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    scheduled_pickup_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_shipping_addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    coordinate_latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    coordinate_longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    successful_delivery_count = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_shipping_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    package_type = table.Column<short>(type: "smallint", nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    dimensions_length_cm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    dimensions_width_cm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    dimensions_height_cm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_packages", x => x.id);
                    table.ForeignKey(
                        name: "fk_packages_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipment_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipment_events_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipment_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipment_items_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipping_provider_profiles",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    contact_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    office_address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    office_address_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    office_address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    office_address_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    office_address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    office_address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    service_areas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipping_provider_profiles", x => x.provider_id);
                    table.ForeignKey(
                        name: "fk_shipping_provider_profiles_shipping_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "shipping_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transportation_assignments",
                columns: table => new
                {
                    transportation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_assignments", x => x.transportation_id);
                    table.ForeignKey(
                        name: "fk_transportation_assignments_transportations_transportation_id",
                        column: x => x.transportation_id,
                        principalTable: "transportations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transportation_costs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transportation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    incurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_costs", x => x.id);
                    table.ForeignKey(
                        name: "fk_transportation_costs_transportations_transportation_id",
                        column: x => x.transportation_id,
                        principalTable: "transportations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transportation_proofs",
                columns: table => new
                {
                    transportation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    signature_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_proofs", x => x.transportation_id);
                    table.ForeignKey(
                        name: "fk_transportation_proofs_transportations_transportation_id",
                        column: x => x.transportation_id,
                        principalTable: "transportations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transportation_trackings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transportation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    coordinate_latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    coordinate_longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportation_trackings", x => x.id);
                    table.ForeignKey(
                        name: "fk_transportation_trackings_transportations_transportation_id",
                        column: x => x.transportation_id,
                        principalTable: "transportations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_item_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_package_items_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_carrier_integrations_integration_code",
                table: "carrier_integrations",
                column: "integration_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carrier_integrations_shipping_provider_id",
                table: "carrier_integrations",
                column: "shipping_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carrier_integrations_tenant_id",
                table: "carrier_integrations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_deliveries_status_created_at",
                table: "deliveries",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_deliveries_tenant_id",
                table: "deliveries",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_deliveries_transportation_id",
                table: "deliveries",
                column: "transportation_id",
                unique: true);

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
                name: "idx_outbox_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_package_items_package_id",
                table: "package_items",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_package_items_package_id_shipment_item_id",
                table: "package_items",
                columns: new[] { "package_id", "shipment_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_package_items_shipment_item_id",
                table: "package_items",
                column: "shipment_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_packages_shipment_id",
                table: "packages",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_packages_shipment_id_package_code",
                table: "packages",
                columns: new[] { "shipment_id", "package_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pickups_shipment_id",
                table: "pickups",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_pickups_status_scheduled_at",
                table: "pickups",
                columns: new[] { "status", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "ix_pickups_tenant_id",
                table: "pickups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_shipments_original_shipment_id",
                table: "return_shipments",
                column: "original_shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_shipments_returned_shipment_id",
                table: "return_shipments",
                column: "returned_shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_shipments_status_requested_at",
                table: "return_shipments",
                columns: new[] { "status", "requested_at" });

            migrationBuilder.CreateIndex(
                name: "ix_return_shipments_tenant_id",
                table: "return_shipments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipment_events_shipment_id_occurred_at",
                table: "shipment_events",
                columns: new[] { "shipment_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shipment_items_shipment_id",
                table: "shipment_items",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipment_items_shipment_id_line_no",
                table: "shipment_items",
                columns: new[] { "shipment_id", "line_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipments_created_at",
                table: "shipments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_idempotency_key",
                table: "shipments",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_shipment_number",
                table: "shipments",
                column: "shipment_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipments_source_type_source_reference_id",
                table: "shipments",
                columns: new[] { "source_type", "source_reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_shipments_status_created_at",
                table: "shipments",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shipments_tenant_id",
                table: "shipments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_profiles_tenant_id",
                table: "shipping_profiles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_profiles_user_id",
                table: "shipping_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_profiles_user_id_is_default",
                table: "shipping_profiles",
                columns: new[] { "user_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_shipping_profiles_verified_address_id",
                table: "shipping_profiles",
                column: "verified_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_providers_code",
                table: "shipping_providers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipping_providers_provider_type_is_active",
                table: "shipping_providers",
                columns: new[] { "provider_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_shipping_providers_tenant_id",
                table: "shipping_providers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_assignments_person_id",
                table: "transportation_assignments",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_assignments_vehicle_id",
                table: "transportation_assignments",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_cost_rules_code",
                table: "transportation_cost_rules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportation_cost_rules_is_active_effective_from",
                table: "transportation_cost_rules",
                columns: new[] { "is_active", "effective_from" });

            migrationBuilder.CreateIndex(
                name: "ix_transportation_cost_rules_provider_id",
                table: "transportation_cost_rules",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_cost_rules_tenant_id",
                table: "transportation_cost_rules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_costs_transportation_id",
                table: "transportation_costs",
                column: "transportation_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_costs_transportation_id_category",
                table: "transportation_costs",
                columns: new[] { "transportation_id", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_transportation_people_provider_id",
                table: "transportation_people",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_people_provider_id_status",
                table: "transportation_people",
                columns: new[] { "provider_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_transportation_people_tenant_id",
                table: "transportation_people",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_people_user_id",
                table: "transportation_people",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportation_trackings_transportation_id_recorded_at",
                table: "transportation_trackings",
                columns: new[] { "transportation_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "ix_transportation_vehicles_plate_number",
                table: "transportation_vehicles",
                column: "plate_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportation_vehicles_provider_id_status",
                table: "transportation_vehicles",
                columns: new[] { "provider_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_transportation_vehicles_tenant_id",
                table: "transportation_vehicles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportations_cost_rule_id",
                table: "transportations",
                column: "cost_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportations_idempotency_key",
                table: "transportations",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_transportations_provider_id",
                table: "transportations",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportations_shipment_id",
                table: "transportations",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportations_shipment_id_attempt_no",
                table: "transportations",
                columns: new[] { "shipment_id", "attempt_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportations_status_created_at",
                table: "transportations",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_transportations_tenant_id",
                table: "transportations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transportations_transportation_number",
                table: "transportations",
                column: "transportation_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_shipping_addresses_tenant_id",
                table: "verified_shipping_addresses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_shipping_addresses_user_id",
                table: "verified_shipping_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_shipping_addresses_user_id_status",
                table: "verified_shipping_addresses",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrier_integrations");

            migrationBuilder.DropTable(
                name: "deliveries");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "inbox_retry_histories");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "package_items");

            migrationBuilder.DropTable(
                name: "pickups");

            migrationBuilder.DropTable(
                name: "return_shipments");

            migrationBuilder.DropTable(
                name: "shipment_events");

            migrationBuilder.DropTable(
                name: "shipment_items");

            migrationBuilder.DropTable(
                name: "shipping_profiles");

            migrationBuilder.DropTable(
                name: "shipping_provider_profiles");

            migrationBuilder.DropTable(
                name: "transportation_assignments");

            migrationBuilder.DropTable(
                name: "transportation_cost_rules");

            migrationBuilder.DropTable(
                name: "transportation_costs");

            migrationBuilder.DropTable(
                name: "transportation_people");

            migrationBuilder.DropTable(
                name: "transportation_proofs");

            migrationBuilder.DropTable(
                name: "transportation_trackings");

            migrationBuilder.DropTable(
                name: "transportation_vehicles");

            migrationBuilder.DropTable(
                name: "verified_shipping_addresses");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "shipping_providers");

            migrationBuilder.DropTable(
                name: "transportations");

            migrationBuilder.DropTable(
                name: "shipments");
        }
    }
}
