using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Product.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductVariationToVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_variations_products_product_id",
                table: "product_variations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_product_variations",
                table: "product_variations");

            migrationBuilder.RenameTable(
                name: "product_variations",
                newName: "variants");

            migrationBuilder.RenameIndex(
                name: "ix_product_variations_product_id",
                table: "variants",
                newName: "ix_variants_product_id");

            migrationBuilder.RenameIndex(
                name: "ix_product_variations_sku",
                table: "variants",
                newName: "ix_variants_sku");

            migrationBuilder.AddPrimaryKey(
                name: "pk_variants",
                table: "variants",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_variants_products_product_id",
                table: "variants",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_variants_products_product_id",
                table: "variants");

            migrationBuilder.DropPrimaryKey(
                name: "pk_variants",
                table: "variants");

            migrationBuilder.RenameIndex(
                name: "ix_variants_sku",
                table: "variants",
                newName: "ix_product_variations_sku");

            migrationBuilder.RenameIndex(
                name: "ix_variants_product_id",
                table: "variants",
                newName: "ix_product_variations_product_id");

            migrationBuilder.RenameTable(
                name: "variants",
                newName: "product_variations");

            migrationBuilder.AddPrimaryKey(
                name: "pk_product_variations",
                table: "product_variations",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_variations_products_product_id",
                table: "product_variations",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
