using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPriceOrderTaxOrderStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_total",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "grand_total",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_discount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "subtotal",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tax_applied_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tax_method",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tax_value",
                table: "orders");

            migrationBuilder.CreateTable(
                name: "order_prices",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    exchange_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    item_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    promotion_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    coupon_discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    service_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    platform_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    rounding_adjustment = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_prices", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_prices_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_status_histories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    previous_status = table.Column<int>(type: "integer", nullable: true),
                    current_status = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_order_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_status_histories_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_taxes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tax_type = table.Column<int>(type: "integer", nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_taxes", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_taxes_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_prices_tenant_id",
                table: "order_prices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_order_id_changed_at",
                table: "order_status_histories",
                columns: new[] { "order_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_tenant_id",
                table: "order_status_histories",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_taxes_order_id",
                table: "order_taxes",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_taxes_tenant_id",
                table: "order_taxes",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_prices");

            migrationBuilder.DropTable(
                name: "order_status_histories");

            migrationBuilder.DropTable(
                name: "order_taxes");

            migrationBuilder.AddColumn<decimal>(
                name: "discount_total",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "grand_total",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_discount",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "subtotal",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_applied_amount",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tax_method",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_value",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
