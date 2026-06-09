using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend_api_dotnet9.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_design_files_DesignFileId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_materials_MaterialId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_print_types_PrintTypeId",
                table: "order_items");

            migrationBuilder.DropTable(
                name: "design_files");

            migrationBuilder.DropTable(
                name: "product_materials");

            migrationBuilder.DropTable(
                name: "product_print_options");

            migrationBuilder.DropIndex(
                name: "IX_order_items_DesignFileId",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_order_items_MaterialId",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_order_items_PrintTypeId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "DesignFileId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "PrintTypeId",
                table: "order_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesignFileId",
                table: "order_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "order_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrintTypeId",
                table: "order_items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "design_files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    uploaded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_design_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_design_files_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_materials_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_materials_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_print_options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrintTypeId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_print_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_print_options_print_types_PrintTypeId",
                        column: x => x.PrintTypeId,
                        principalTable: "print_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_print_options_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_DesignFileId",
                table: "order_items",
                column: "DesignFileId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_MaterialId",
                table: "order_items",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_PrintTypeId",
                table: "order_items",
                column: "PrintTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_design_files_ProductId",
                table: "design_files",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_materials_MaterialId",
                table: "product_materials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_product_materials_ProductId_MaterialId",
                table: "product_materials",
                columns: new[] { "ProductId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_print_options_PrintTypeId",
                table: "product_print_options",
                column: "PrintTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_product_print_options_ProductId_PrintTypeId",
                table: "product_print_options",
                columns: new[] { "ProductId", "PrintTypeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_design_files_DesignFileId",
                table: "order_items",
                column: "DesignFileId",
                principalTable: "design_files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_materials_MaterialId",
                table: "order_items",
                column: "MaterialId",
                principalTable: "materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_print_types_PrintTypeId",
                table: "order_items",
                column: "PrintTypeId",
                principalTable: "print_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
