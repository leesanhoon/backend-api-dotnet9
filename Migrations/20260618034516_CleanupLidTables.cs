using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_api_dotnet9.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLidTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Drop leftover lid tables if they still exist
                DROP TABLE IF EXISTS lid_images CASCADE;
                DROP TABLE IF EXISTS lid_prices CASCADE;
                DROP TABLE IF EXISTS lids CASCADE;

                -- Drop temp mapping table if left behind
                DROP TABLE IF EXISTS lid_to_product_map;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
