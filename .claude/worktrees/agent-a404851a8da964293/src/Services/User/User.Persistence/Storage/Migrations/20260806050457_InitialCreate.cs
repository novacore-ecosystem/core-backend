using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.User.Persistence.Storage.Migrations
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
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    permissions = table.Column<string[]>(type: "text[]", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    scope = table.Column<short>(type: "smallint", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)5),
                    user_type = table.Column<short>(type: "smallint", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_role_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_translations", x => new { x.id, x.language_code });
                    table.ForeignKey(
                        name: "fk_user_role_translations_user_roles_id",
                        column: x => x.id,
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tag_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tag_translations", x => new { x.id, x.language_code });
                    table.ForeignKey(
                        name: "fk_user_tag_translations_user_tags_id",
                        column: x => x.id,
                        principalTable: "user_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_activity_summaries",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_order_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_purchase_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_login_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_order_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_spent_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    favorite_category = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_activity_summaries", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_activity_summaries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    receiver_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    receiver_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    receiver_company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    apartment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    delivery_instruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address_type = table.Column<short>(type: "smallint", nullable: false),
                    is_default_shipping = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_default_billing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_avatars",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thumbnail_media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_mode = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_avatars", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_avatars_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_type = table.Column<short>(type: "smallint", nullable: false),
                    value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_contacts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sms_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    push_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    signal_r_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    marketing_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    order_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    promotion_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    security_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_notification_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_notification_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<short>(type: "smallint", nullable: false),
                    payment_type = table.Column<short>(type: "smallint", nullable: false),
                    external_customer_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    external_payment_method_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    card_holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    card_masked_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    card_last4_digits = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    card_expire_month = table.Column<int>(type: "integer", nullable: true),
                    card_expire_year = table.Column<int>(type: "integer", nullable: true),
                    card_brand = table.Column<short>(type: "smallint", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_payment_methods", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_payment_methods_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_permission_snapshots",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permissions = table.Column<string[]>(type: "text[]", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_permission_snapshots", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_permission_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    favorite_categories = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    favorite_brands = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    preferred_warehouse_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    recently_viewed_products = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    search_history = table.Column<string[]>(type: "text[]", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_preferences", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_privacy_settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    show_birthday = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    show_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    show_phone_number = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    allow_tracking = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    allow_recommendation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    allow_personalized_ads = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_privacy_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_privacy_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    biography = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    occupation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_role_assignments_user_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_role_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_security_settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    require_password_rotation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    allow_remember_device = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    trusted_devices_only = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recovery_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    recovery_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_security_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_security_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    date_format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    time_format = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    first_day_of_week = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    dashboard_layout = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sidebar_collapsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    items_per_page = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tag_mappings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tag_mappings", x => new { x.user_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_user_tag_mappings_user_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "user_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_tag_mappings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verification_type = table.Column<short>(type: "smallint", nullable: false),
                    verification_status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_verifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_verifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
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
                name: "idx_outbox_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_activity_summaries_favorite_category",
                table: "user_activity_summaries",
                column: "favorite_category");

            migrationBuilder.CreateIndex(
                name: "ix_user_activity_summaries_tenant_id",
                table: "user_activity_summaries",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_addresses_tenant_id",
                table: "user_addresses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_addresses_user_id",
                table: "user_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_addresses_user_id_is_default_billing",
                table: "user_addresses",
                columns: new[] { "user_id", "is_default_billing" });

            migrationBuilder.CreateIndex(
                name: "ix_user_addresses_user_id_is_default_shipping",
                table: "user_addresses",
                columns: new[] { "user_id", "is_default_shipping" });

            migrationBuilder.CreateIndex(
                name: "ix_user_avatars_tenant_id",
                table: "user_avatars",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_contacts_tenant_id",
                table: "user_contacts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_contacts_user_id_contact_type_is_primary",
                table: "user_contacts",
                columns: new[] { "user_id", "contact_type", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "ix_user_contacts_user_id_contact_type_value",
                table: "user_contacts",
                columns: new[] { "user_id", "contact_type", "value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_notification_settings_tenant_id",
                table: "user_notification_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_payment_methods_provider_external_payment_method_id",
                table: "user_payment_methods",
                columns: new[] { "provider", "external_payment_method_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_payment_methods_tenant_id",
                table: "user_payment_methods",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_payment_methods_user_id",
                table: "user_payment_methods",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_payment_methods_user_id_is_default",
                table: "user_payment_methods",
                columns: new[] { "user_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_user_permission_snapshots_permissions",
                table: "user_permission_snapshots",
                column: "permissions")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_user_permission_snapshots_tenant_id",
                table: "user_permission_snapshots",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_preferences_tenant_id",
                table: "user_preferences",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_privacy_settings_tenant_id",
                table: "user_privacy_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_tenant_id",
                table: "user_profiles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_role_id",
                table: "user_role_assignments",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_tenant_id",
                table: "user_role_assignments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_user_id",
                table: "user_role_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_user_id_role_id_status",
                table: "user_role_assignments",
                columns: new[] { "user_id", "role_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_user_role_translations_tenant_id",
                table: "user_role_translations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_key",
                table: "user_roles",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_permissions",
                table: "user_roles",
                column: "permissions")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_status",
                table: "user_roles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_tenant_id",
                table: "user_roles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_security_settings_tenant_id",
                table: "user_security_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_settings_tenant_id",
                table: "user_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tag_mappings_tag_id",
                table: "user_tag_mappings",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tag_mappings_tenant_id",
                table: "user_tag_mappings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tag_translations_tenant_id",
                table: "user_tag_translations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tags_name",
                table: "user_tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_tags_scope",
                table: "user_tags",
                column: "scope");

            migrationBuilder.CreateIndex(
                name: "ix_user_tags_tenant_id",
                table: "user_tags",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_verifications_tenant_id",
                table: "user_verifications",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_verifications_user_id_verification_type_verification_s",
                table: "user_verifications",
                columns: new[] { "user_id", "verification_type", "verification_status" });

            migrationBuilder.CreateIndex(
                name: "ix_users_is_deleted",
                table: "users",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_users_status",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant_id",
                table: "users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_user_type",
                table: "users",
                column: "user_type");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "inbox_retry_histories");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "user_activity_summaries");

            migrationBuilder.DropTable(
                name: "user_addresses");

            migrationBuilder.DropTable(
                name: "user_avatars");

            migrationBuilder.DropTable(
                name: "user_contacts");

            migrationBuilder.DropTable(
                name: "user_notification_settings");

            migrationBuilder.DropTable(
                name: "user_payment_methods");

            migrationBuilder.DropTable(
                name: "user_permission_snapshots");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "user_privacy_settings");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_role_assignments");

            migrationBuilder.DropTable(
                name: "user_role_translations");

            migrationBuilder.DropTable(
                name: "user_security_settings");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "user_tag_mappings");

            migrationBuilder.DropTable(
                name: "user_tag_translations");

            migrationBuilder.DropTable(
                name: "user_verifications");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tags");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
