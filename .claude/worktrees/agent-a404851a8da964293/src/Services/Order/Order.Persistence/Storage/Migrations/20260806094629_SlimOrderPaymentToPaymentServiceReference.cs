using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class SlimOrderPaymentToPaymentServiceReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_order_payments_reference_number",
                table: "order_payments");

            migrationBuilder.DropColumn(
                name: "masked_account",
                table: "order_payments");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "order_payments");

            migrationBuilder.DropColumn(
                name: "payment_provider",
                table: "order_payments");

            migrationBuilder.DropColumn(
                name: "provider_name",
                table: "order_payments");

            migrationBuilder.DropColumn(
                name: "reference_number",
                table: "order_payments");

            migrationBuilder.RenameColumn(
                name: "payment_reference_id",
                table: "order_payments",
                newName: "payment_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_payments_payment_reference_id",
                table: "order_payments",
                newName: "ix_order_payments_payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payment_id",
                table: "order_payments",
                newName: "payment_reference_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_payments_payment_id",
                table: "order_payments",
                newName: "ix_order_payments_payment_reference_id");

            migrationBuilder.AddColumn<string>(
                name: "masked_account",
                table: "order_payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "payment_method",
                table: "order_payments",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "payment_provider",
                table: "order_payments",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_name",
                table: "order_payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_number",
                table: "order_payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_reference_number",
                table: "order_payments",
                column: "reference_number");
        }
    }
}
