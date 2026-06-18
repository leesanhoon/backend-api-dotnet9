using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_api_dotnet9.Migrations
{
    /// <inheritdoc />
    public partial class AddLidIdToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LidId",
                table: "order_items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_items_LidId",
                table: "order_items",
                column: "LidId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_lids_LidId",
                table: "order_items",
                column: "LidId",
                principalTable: "lids",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_lids_LidId",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_order_items_LidId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "LidId",
                table: "order_items");
        }
    }
}
