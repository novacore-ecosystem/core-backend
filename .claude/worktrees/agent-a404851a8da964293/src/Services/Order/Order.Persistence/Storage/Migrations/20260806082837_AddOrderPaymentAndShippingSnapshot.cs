using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentAndShippingSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "arrived_at_warehouse_at",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "delivered_at",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "final_fee",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "in_transit_at",
                table: "order_shippings");

            migrationBuilder.RenameColumn(
                name: "shipped_at",
                table: "order_shippings",
                newName: "estimated_delivery");

            migrationBuilder.RenameColumn(
                name: "original_fee",
                table: "order_shippings",
                newName: "shipping_fee");

            migrationBuilder.AddColumn<string>(
                name: "carrier",
                table: "order_shippings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "shipping_reference_id",
                table: "order_shippings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tracking_number",
                table: "order_shippings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "order_payments",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_method = table.Column<short>(type: "smallint", nullable: true),
                    payment_provider = table.Column<short>(type: "smallint", nullable: true),
                    provider_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    masked_account = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_payments", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_shippings_tracking_number",
                table: "order_shippings",
                column: "tracking_number");

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_payment_reference_id",
                table: "order_payments",
                column: "payment_reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_reference_number",
                table: "order_payments",
                column: "reference_number");

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_tenant_id",
                table: "order_payments",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_payments");

            migrationBuilder.DropIndex(
                name: "ix_order_shippings_tracking_number",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "carrier",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "shipping_reference_id",
                table: "order_shippings");

            migrationBuilder.DropColumn(
                name: "tracking_number",
                table: "order_shippings");

            migrationBuilder.RenameColumn(
                name: "shipping_fee",
                table: "order_shippings",
                newName: "original_fee");

            migrationBuilder.RenameColumn(
                name: "estimated_delivery",
                table: "order_shippings",
                newName: "shipped_at");

            migrationBuilder.AddColumn<DateTime>(
                name: "arrived_at_warehouse_at",
                table: "order_shippings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "delivered_at",
                table: "order_shippings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "order_shippings",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "final_fee",
                table: "order_shippings",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "in_transit_at",
                table: "order_shippings",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
