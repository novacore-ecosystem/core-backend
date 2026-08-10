using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Payment.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "billing_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    to_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    effective_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    request_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    response_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotency_records", x => x.id);
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
                name: "payment_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_type = table.Column<short>(type: "smallint", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    masked_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    expiration_month = table.Column<int>(type: "integer", nullable: true),
                    expiration_year = table.Column<int>(type: "integer", nullable: true),
                    issuer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_audits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    actor_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_audits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_event_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<short>(type: "smallint", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<short>(type: "smallint", nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_event_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_fees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    settlement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fee_type = table.Column<short>(type: "smallint", nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_fees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_gateways",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gateway_type = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_gateways", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_intents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<short>(type: "smallint", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    requested_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    client_secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_intents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    method_type = table.Column<short>(type: "smallint", nullable: false),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    channel = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payouts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payee_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payouts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<short>(type: "smallint", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    frequency = table.Column<short>(type: "smallint", nullable: false),
                    next_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scheduled_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gross_amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    gross_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fee_amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fee_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    net_amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    net_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settlements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_deliveries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    signature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<short>(type: "smallint", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoices_billing_profiles_billing_profile_id",
                        column: x => x.billing_profile_id,
                        principalTable: "billing_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payment_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    token_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_tokens_payment_accounts_payment_account_id",
                        column: x => x.payment_account_id,
                        principalTable: "payment_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gateway_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment = table.Column<short>(type: "smallint", nullable: false),
                    api_key_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    secret_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    webhook_secret_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gateway_configurations", x => x.id);
                    table.ForeignKey(
                        name: "fk_gateway_configurations_payment_gateways_gateway_id",
                        column: x => x.gateway_id,
                        principalTable: "payment_gateways",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gateway_status_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_status_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mapped_status = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gateway_status_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_gateway_status_mappings_payment_gateways_gateway_id",
                        column: x => x.gateway_id,
                        principalTable: "payment_gateways",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_intent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    redirect_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    return_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_sessions_payment_intents_payment_intent_id",
                        column: x => x.payment_intent_id,
                        principalTable: "payment_intents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reference_type = table.Column<short>(type: "smallint", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_payment_intents_payment_intent_id",
                        column: x => x.payment_intent_id,
                        principalTable: "payment_intents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reconciliations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    discrepancy_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reconciled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliations_settlements_settlement_id",
                        column: x => x.settlement_id,
                        principalTable: "settlements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    gateway_transaction_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gateway_order_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    request_payload = table.Column<string>(type: "jsonb", nullable: true),
                    response_payload = table.Column<string>(type: "jsonb", nullable: true),
                    response_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    failure_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    redirect_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    authorized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_attempts_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_items_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<short>(type: "smallint", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refunds", x => x.id);
                    table.ForeignKey(
                        name: "fk_refunds_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refund_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    gateway_refund_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    response_payload = table.Column<string>(type: "jsonb", nullable: true),
                    failure_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refund_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_refund_attempts_refunds_refund_id",
                        column: x => x.refund_id,
                        principalTable: "refunds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_billing_profiles_owner_reference_id",
                table: "billing_profiles",
                column: "owner_reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_billing_profiles_tenant_id",
                table: "billing_profiles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_from_currency_to_currency_effective_at",
                table: "exchange_rates",
                columns: new[] { "from_currency", "to_currency", "effective_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gateway_configurations_gateway_id_environment",
                table: "gateway_configurations",
                columns: new[] { "gateway_id", "environment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gateway_status_mappings_gateway_id_gateway_status_code",
                table: "gateway_status_mappings",
                columns: new[] { "gateway_id", "gateway_status_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_records_expires_at",
                table: "idempotency_records",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_records_key",
                table: "idempotency_records",
                column: "key",
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
                name: "ix_invoices_billing_profile_id",
                table: "invoices",
                column: "billing_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_reference_type_reference_id",
                table: "invoices",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_invoices_status",
                table: "invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_tenant_id",
                table: "invoices",
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
                name: "ix_payment_accounts_owner_reference_id",
                table: "payment_accounts",
                column: "owner_reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_accounts_owner_reference_id_is_default",
                table: "payment_accounts",
                columns: new[] { "owner_reference_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_accounts_tenant_id",
                table: "payment_accounts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_attempts_gateway_transaction_id",
                table: "payment_attempts",
                column: "gateway_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_attempts_payment_id",
                table: "payment_attempts",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_attempts_payment_id_attempt_number",
                table: "payment_attempts",
                columns: new[] { "payment_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_audits_occurred_at",
                table: "payment_audits",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_audits_payment_id",
                table: "payment_audits",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_event_logs_occurred_at",
                table: "payment_event_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_event_logs_payment_id",
                table: "payment_event_logs",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_event_logs_reference_type_reference_id",
                table: "payment_event_logs",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_fees_payment_id",
                table: "payment_fees",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_fees_settlement_id",
                table: "payment_fees",
                column: "settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_gateways_code",
                table: "payment_gateways",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_intents_client_secret",
                table: "payment_intents",
                column: "client_secret",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_intents_created_at",
                table: "payment_intents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_intents_reference_type_reference_id",
                table: "payment_intents",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_intents_tenant_id",
                table: "payment_intents",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_items_payment_id",
                table: "payment_items",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_code",
                table: "payment_methods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_notifications_payment_id",
                table: "payment_notifications",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_notifications_status",
                table: "payment_notifications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_sessions_payment_intent_id",
                table: "payment_sessions",
                column: "payment_intent_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_tokens_payment_account_id",
                table: "payment_tokens",
                column: "payment_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_tokens_payment_account_id_gateway_id",
                table: "payment_tokens",
                columns: new[] { "payment_account_id", "gateway_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_gateway_id",
                table: "payments",
                column: "gateway_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_idempotency_key",
                table: "payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_payment_intent_id",
                table: "payments",
                column: "payment_intent_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_reference_type_reference_id",
                table: "payments",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_status_created_at",
                table: "payments",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id",
                table: "payments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_payee_reference_id",
                table: "payouts",
                column: "payee_reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_status",
                table: "payouts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_tenant_id",
                table: "payouts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliations_settlement_id",
                table: "reconciliations",
                column: "settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_attempts_refund_id",
                table: "refund_attempts",
                column: "refund_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_attempts_refund_id_attempt_number",
                table: "refund_attempts",
                columns: new[] { "refund_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refunds_payment_id",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_reference_type_reference_id",
                table: "refunds",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_refunds_status_created_at",
                table: "refunds",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_refunds_tenant_id",
                table: "refunds",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_payments_reference_type_reference_id",
                table: "scheduled_payments",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_payments_status_next_run_at",
                table: "scheduled_payments",
                columns: new[] { "status", "next_run_at" });

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_payments_tenant_id",
                table: "scheduled_payments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_settlements_gateway_id",
                table: "settlements",
                column: "gateway_id");

            migrationBuilder.CreateIndex(
                name: "ix_settlements_period_start_period_end",
                table: "settlements",
                columns: new[] { "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_status",
                table: "webhook_deliveries",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_created_at",
                table: "webhook_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_gateway_id",
                table: "webhook_events",
                column: "gateway_id");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_status",
                table: "webhook_events",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "gateway_configurations");

            migrationBuilder.DropTable(
                name: "gateway_status_mappings");

            migrationBuilder.DropTable(
                name: "idempotency_records");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "inbox_retry_histories");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "payment_attempts");

            migrationBuilder.DropTable(
                name: "payment_audits");

            migrationBuilder.DropTable(
                name: "payment_event_logs");

            migrationBuilder.DropTable(
                name: "payment_fees");

            migrationBuilder.DropTable(
                name: "payment_items");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "payment_notifications");

            migrationBuilder.DropTable(
                name: "payment_sessions");

            migrationBuilder.DropTable(
                name: "payment_tokens");

            migrationBuilder.DropTable(
                name: "payouts");

            migrationBuilder.DropTable(
                name: "reconciliations");

            migrationBuilder.DropTable(
                name: "refund_attempts");

            migrationBuilder.DropTable(
                name: "scheduled_payments");

            migrationBuilder.DropTable(
                name: "webhook_deliveries");

            migrationBuilder.DropTable(
                name: "webhook_events");

            migrationBuilder.DropTable(
                name: "payment_gateways");

            migrationBuilder.DropTable(
                name: "billing_profiles");

            migrationBuilder.DropTable(
                name: "payment_accounts");

            migrationBuilder.DropTable(
                name: "settlements");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "payment_intents");
        }
    }
}
