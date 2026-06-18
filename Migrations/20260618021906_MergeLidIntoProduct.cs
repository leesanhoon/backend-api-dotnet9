using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend_api_dotnet9.Migrations
{
    /// <inheritdoc />
    public partial class MergeLidIntoProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop old FKs that reference lids table
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_lids_LidId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_product_lids_lids_LidId",
                table: "product_lids");

            // 2. Add SizeName column to product_variants
            migrationBuilder.AddColumn<string>(
                name: "SizeName",
                table: "product_variants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // 3. Data migration: lids → products
            migrationBuilder.Sql(@"
                -- Create temp mapping table to track old lid IDs → new product IDs
                CREATE TEMP TABLE lid_to_product_map (
                    old_lid_id integer NOT NULL,
                    new_product_id integer NOT NULL
                );

                -- Insert lids into products and capture the mapping
                INSERT INTO products (""Name"", ""Description"", ""CategoryId"", ""avatar_image_url"")
                SELECT ""Name"", ""Description"", ""CategoryId"", ""avatar_image_url""
                FROM lids;

                -- Build the mapping using row order (lids are inserted in Id order)
                INSERT INTO lid_to_product_map (old_lid_id, new_product_id)
                SELECT l.""Id"", p.""Id""
                FROM (
                    SELECT ""Id"", ""Name"", ""CategoryId"",
                           ROW_NUMBER() OVER (ORDER BY ""Id"") AS rn
                    FROM lids
                ) l
                JOIN (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (ORDER BY ""Id"" DESC) AS rn
                    FROM products
                    WHERE ""Id"" NOT IN (
                        SELECT ""Id"" FROM products
                        WHERE ""Id"" NOT IN (
                            SELECT p2.""Id"" FROM products p2
                            ORDER BY p2.""Id"" DESC
                            LIMIT (SELECT COUNT(*) FROM lids)
                        )
                    )
                ) p ON 1=0;
            ");

            // Simpler mapping approach using name+category match
            migrationBuilder.Sql(@"
                -- Clear and rebuild mapping using a more reliable approach
                DELETE FROM lid_to_product_map;

                -- Use a CTE to match lids to their newly inserted products
                -- Since we just inserted, the newest products with matching name+category are the migrated lids
                WITH ranked_products AS (
                    SELECT p.""Id"" as new_product_id, p.""Name"", p.""CategoryId"",
                           ROW_NUMBER() OVER (PARTITION BY p.""Name"", p.""CategoryId"" ORDER BY p.""Id"" DESC) as rn
                    FROM products p
                ),
                lid_match AS (
                    SELECT l.""Id"" as old_lid_id, rp.new_product_id
                    FROM lids l
                    JOIN ranked_products rp ON rp.""Name"" = l.""Name""
                        AND rp.""CategoryId"" = l.""CategoryId""
                        AND rp.rn = 1
                )
                INSERT INTO lid_to_product_map (old_lid_id, new_product_id)
                SELECT old_lid_id, new_product_id FROM lid_match;
            ");

            // 4. Migrate lid_prices → product_variants + variant_price_tiers
            migrationBuilder.Sql(@"
                -- Insert lid_prices as product_variants (CapacityMl = 0 for lids)
                INSERT INTO product_variants (""ProductId"", ""CapacityMl"", ""DiameterMm"", ""SizeName"")
                SELECT m.new_product_id, 0, lp.""DiameterMm"", lp.""SizeName""
                FROM lid_prices lp
                JOIN lid_to_product_map m ON m.old_lid_id = lp.""LidId"";

                -- Insert corresponding variant_price_tiers (single tier with MinQuantity = 1)
                INSERT INTO variant_price_tiers (""ProductVariantId"", ""MinQuantity"", ""UnitPrice"")
                SELECT pv.""Id"", 1, lp.""UnitPrice""
                FROM lid_prices lp
                JOIN lid_to_product_map m ON m.old_lid_id = lp.""LidId""
                JOIN product_variants pv ON pv.""ProductId"" = m.new_product_id
                    AND pv.""CapacityMl"" = 0
                    AND pv.""DiameterMm"" = lp.""DiameterMm"";
            ");

            // 5. Migrate lid_images → product_images
            migrationBuilder.Sql(@"
                INSERT INTO product_images (""ProductId"", ""ImageType"", ""ImageUrl"", ""DisplayOrder"", ""created_at_utc"")
                SELECT m.new_product_id, li.""ImageType"", li.""ImageUrl"", li.""DisplayOrder"", li.""created_at_utc""
                FROM lid_images li
                JOIN lid_to_product_map m ON m.old_lid_id = li.""LidId"";
            ");

            // 6. Update product_lids.LidId → new product IDs before rename
            migrationBuilder.Sql(@"
                UPDATE product_lids
                SET ""LidId"" = m.new_product_id
                FROM lid_to_product_map m
                WHERE product_lids.""LidId"" = m.old_lid_id;
            ");

            // 7. Update order_items.LidId → new product IDs
            migrationBuilder.Sql(@"
                UPDATE order_items
                SET ""LidId"" = m.new_product_id
                FROM lid_to_product_map m
                WHERE order_items.""LidId"" = m.old_lid_id;
            ");

            // 8. Clean up temp table
            migrationBuilder.Sql("DROP TABLE IF EXISTS lid_to_product_map;");

            // 9. Now safe to drop lid tables
            migrationBuilder.DropTable(
                name: "lid_images");

            migrationBuilder.DropTable(
                name: "lid_prices");

            migrationBuilder.DropTable(
                name: "lids");

            // 10. Update unique index on product_variants
            migrationBuilder.DropIndex(
                name: "IX_product_variants_ProductId_CapacityMl",
                table: "product_variants");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId_CapacityMl_DiameterMm",
                table: "product_variants",
                columns: new[] { "ProductId", "CapacityMl", "DiameterMm" },
                unique: true);

            // 11. Rename LidId → CompatibleProductId in product_lids
            migrationBuilder.RenameColumn(
                name: "LidId",
                table: "product_lids",
                newName: "CompatibleProductId");

            migrationBuilder.RenameIndex(
                name: "IX_product_lids_ProductId_LidId",
                table: "product_lids",
                newName: "IX_product_lids_ProductId_CompatibleProductId");

            migrationBuilder.RenameIndex(
                name: "IX_product_lids_LidId",
                table: "product_lids",
                newName: "IX_product_lids_CompatibleProductId");

            // 12. Add new FKs pointing to products table
            migrationBuilder.AddForeignKey(
                name: "FK_order_items_products_LidId",
                table: "order_items",
                column: "LidId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_lids_products_CompatibleProductId",
                table: "product_lids",
                column: "CompatibleProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_products_LidId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_product_lids_products_CompatibleProductId",
                table: "product_lids");

            migrationBuilder.DropIndex(
                name: "IX_product_variants_ProductId_CapacityMl_DiameterMm",
                table: "product_variants");

            migrationBuilder.DropColumn(
                name: "SizeName",
                table: "product_variants");

            migrationBuilder.RenameColumn(
                name: "CompatibleProductId",
                table: "product_lids",
                newName: "LidId");

            migrationBuilder.RenameIndex(
                name: "IX_product_lids_ProductId_CompatibleProductId",
                table: "product_lids",
                newName: "IX_product_lids_ProductId_LidId");

            migrationBuilder.RenameIndex(
                name: "IX_product_lids_CompatibleProductId",
                table: "product_lids",
                newName: "IX_product_lids_LidId");

            migrationBuilder.CreateTable(
                name: "lids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    avatar_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lids_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lid_images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LidId = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ImageType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lid_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lid_images_lids_LidId",
                        column: x => x.LidId,
                        principalTable: "lids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lid_prices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LidId = table.Column<int>(type: "integer", nullable: false),
                    DiameterMm = table.Column<int>(type: "integer", nullable: false),
                    SizeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lid_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lid_prices_lids_LidId",
                        column: x => x.LidId,
                        principalTable: "lids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId_CapacityMl",
                table: "product_variants",
                columns: new[] { "ProductId", "CapacityMl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lid_images_LidId",
                table: "lid_images",
                column: "LidId");

            migrationBuilder.CreateIndex(
                name: "IX_lid_prices_LidId_DiameterMm",
                table: "lid_prices",
                columns: new[] { "LidId", "DiameterMm" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lids_CategoryId",
                table: "lids",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_lids_LidId",
                table: "order_items",
                column: "LidId",
                principalTable: "lids",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_lids_lids_LidId",
                table: "product_lids",
                column: "LidId",
                principalTable: "lids",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
