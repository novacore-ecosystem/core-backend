using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.User.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class SlimUserPaymentMethodToPaymentServiceReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_payment_methods_provider_external_payment_method_id",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "card_brand",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "card_expire_month",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "card_expire_year",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "card_holder_name",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "card_last4_digits",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "card_masked_number",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "external_customer_id",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "external_payment_method_id",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "payment_type",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "provider",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "token",
                table: "user_payment_methods");

            migrationBuilder.AddColumn<Guid>(
                name: "payment_account_id",
                table: "user_payment_methods",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_user_payment_methods_payment_account_id",
                table: "user_payment_methods",
                column: "payment_account_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_payment_methods_payment_account_id",
                table: "user_payment_methods");

            migrationBuilder.DropColumn(
                name: "payment_account_id",
                table: "user_payment_methods");

            migrationBuilder.AddColumn<short>(
                name: "card_brand",
                table: "user_payment_methods",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "card_expire_month",
                table: "user_payment_methods",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "card_expire_year",
                table: "user_payment_methods",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "card_holder_name",
                table: "user_payment_methods",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "card_last4_digits",
                table: "user_payment_methods",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "card_masked_number",
                table: "user_payment_methods",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_customer_id",
                table: "user_payment_methods",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_payment_method_id",
                table: "user_payment_methods",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "user_payment_methods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "payment_type",
                table: "user_payment_methods",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "provider",
                table: "user_payment_methods",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "user_payment_methods",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_user_payment_methods_provider_external_payment_method_id",
                table: "user_payment_methods",
                columns: new[] { "provider", "external_payment_method_id" },
                unique: true);
        }
    }
}
