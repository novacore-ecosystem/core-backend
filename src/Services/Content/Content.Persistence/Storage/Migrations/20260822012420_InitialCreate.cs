using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Content.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_taxonomies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_taxonomies", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_taxonomies_content_taxonomies_parent_id",
                        column: x => x.parent_id,
                        principalTable: "content_taxonomies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_workflow_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_workflow_definitions", x => x.id);
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
                name: "content_field_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    field_type = table.Column<byte>(type: "smallint", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_localized = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_searchable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_sortable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    default_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    validation_configuration = table.Column<string>(type: "jsonb", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_field_definitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_field_definitions_content_types_content_type_id",
                        column: x => x.content_type_id,
                        principalTable: "content_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_workflow_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_initial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_workflow_states", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_workflow_states_content_workflow_definitions_workfl",
                        column: x => x.workflow_definition_id,
                        principalTable: "content_workflow_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_workflow_transitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_state_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_state_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_workflow_transitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_workflow_transitions_content_workflow_definitions_w",
                        column: x => x.workflow_definition_id,
                        principalTable: "content_workflow_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_content_workflow_transitions_content_workflow_states_from_s",
                        column: x => x.from_state_id,
                        principalTable: "content_workflow_states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_content_workflow_transitions_content_workflow_states_to_sta",
                        column: x => x.to_state_id,
                        principalTable: "content_workflow_states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_audiences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audience_type = table.Column<byte>(type: "smallint", nullable: false),
                    audience_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_audiences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_contributors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<byte>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_contributors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_localizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    culture = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_localizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_publications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    unpublished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_publications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_type = table.Column<byte>(type: "smallint", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_relationships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_taxonomy_assignments",
                columns: table => new
                {
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    taxonomy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_taxonomy_assignments", x => new { x.content_id, x.taxonomy_id });
                    table.ForeignKey(
                        name: "fk_content_taxonomy_assignments_content_taxonomies_taxonomy_id",
                        column: x => x.taxonomy_id,
                        principalTable: "content_taxonomies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_versions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    visibility = table.Column<byte>(type: "smallint", nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    published_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contents", x => x.id);
                    table.ForeignKey(
                        name: "fk_contents_content_types_content_type_id",
                        column: x => x.content_type_id,
                        principalTable: "content_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contents_content_versions_current_version_id",
                        column: x => x.current_version_id,
                        principalTable: "content_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_contents_content_versions_published_version_id",
                        column: x => x.published_version_id,
                        principalTable: "content_versions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "content_workflow_instances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    started_by = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_workflow_instances", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_workflow_instances_content_workflow_definitions_wor",
                        column: x => x.workflow_definition_id,
                        principalTable: "content_workflow_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_content_workflow_instances_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_audiences_content_id",
                table: "content_audiences",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_audiences_tenant_id",
                table: "content_audiences",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_contributors_content_id_user_id",
                table: "content_contributors",
                columns: new[] { "content_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_content_contributors_tenant_id",
                table: "content_contributors",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_field_definitions_content_type_id_key",
                table: "content_field_definitions",
                columns: new[] { "content_type_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_field_definitions_tenant_id",
                table: "content_field_definitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_content_id_culture",
                table: "content_localizations",
                columns: new[] { "content_id", "culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_tenant_id",
                table: "content_localizations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_localizations_version_id",
                table: "content_localizations",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_publications_content_id",
                table: "content_publications",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_publications_status",
                table: "content_publications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_content_publications_tenant_id",
                table: "content_publications",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_publications_version_id",
                table: "content_publications",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_relationships_source_content_id",
                table: "content_relationships",
                column: "source_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_relationships_target_type_target_id",
                table: "content_relationships",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_content_relationships_tenant_id",
                table: "content_relationships",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_taxonomies_key",
                table: "content_taxonomies",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_taxonomies_parent_id",
                table: "content_taxonomies",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_taxonomies_tenant_id",
                table: "content_taxonomies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_taxonomy_assignments_taxonomy_id",
                table: "content_taxonomy_assignments",
                column: "taxonomy_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_taxonomy_assignments_tenant_id",
                table: "content_taxonomy_assignments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_types_key",
                table: "content_types",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_types_status",
                table: "content_types",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_content_types_tenant_id",
                table: "content_types",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_versions_content_id_version_number",
                table: "content_versions",
                columns: new[] { "content_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_versions_tenant_id",
                table: "content_versions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_definitions_key",
                table: "content_workflow_definitions",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_definitions_status",
                table: "content_workflow_definitions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_definitions_tenant_id",
                table: "content_workflow_definitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_instances_content_id",
                table: "content_workflow_instances",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_instances_tenant_id",
                table: "content_workflow_instances",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_instances_workflow_definition_id",
                table: "content_workflow_instances",
                column: "workflow_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_states_tenant_id",
                table: "content_workflow_states",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_states_workflow_definition_id_key",
                table: "content_workflow_states",
                columns: new[] { "workflow_definition_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_transitions_from_state_id_to_state_id",
                table: "content_workflow_transitions",
                columns: new[] { "from_state_id", "to_state_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_transitions_tenant_id",
                table: "content_workflow_transitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_transitions_to_state_id",
                table: "content_workflow_transitions",
                column: "to_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_workflow_transitions_workflow_definition_id",
                table: "content_workflow_transitions",
                column: "workflow_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_contents_content_type_id",
                table: "contents",
                column: "content_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_contents_current_version_id",
                table: "contents",
                column: "current_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_contents_published_version_id",
                table: "contents",
                column: "published_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_contents_slug",
                table: "contents",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contents_status",
                table: "contents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_contents_tenant_id",
                table: "contents",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_contents_visibility",
                table: "contents",
                column: "visibility");

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

            migrationBuilder.AddForeignKey(
                name: "fk_content_audiences_contents_content_id",
                table: "content_audiences",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_contributors_contents_content_id",
                table: "content_contributors",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_localizations_content_versions_version_id",
                table: "content_localizations",
                column: "version_id",
                principalTable: "content_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_content_localizations_contents_content_id",
                table: "content_localizations",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_publications_content_versions_version_id",
                table: "content_publications",
                column: "version_id",
                principalTable: "content_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_content_publications_contents_content_id",
                table: "content_publications",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_relationships_contents_source_content_id",
                table: "content_relationships",
                column: "source_content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_taxonomy_assignments_contents_content_id",
                table: "content_taxonomy_assignments",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_versions_contents_content_id",
                table: "content_versions",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content_versions_contents_content_id",
                table: "content_versions");

            migrationBuilder.DropTable(
                name: "content_audiences");

            migrationBuilder.DropTable(
                name: "content_contributors");

            migrationBuilder.DropTable(
                name: "content_field_definitions");

            migrationBuilder.DropTable(
                name: "content_localizations");

            migrationBuilder.DropTable(
                name: "content_publications");

            migrationBuilder.DropTable(
                name: "content_relationships");

            migrationBuilder.DropTable(
                name: "content_taxonomy_assignments");

            migrationBuilder.DropTable(
                name: "content_workflow_instances");

            migrationBuilder.DropTable(
                name: "content_workflow_transitions");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "inbox_retry_histories");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "content_taxonomies");

            migrationBuilder.DropTable(
                name: "content_workflow_states");

            migrationBuilder.DropTable(
                name: "content_workflow_definitions");

            migrationBuilder.DropTable(
                name: "contents");

            migrationBuilder.DropTable(
                name: "content_types");

            migrationBuilder.DropTable(
                name: "content_versions");
        }
    }
}
