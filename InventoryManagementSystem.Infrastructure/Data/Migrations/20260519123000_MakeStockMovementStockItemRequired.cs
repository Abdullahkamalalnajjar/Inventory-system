using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeStockMovementStockItemRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[StockMovements]', N'U') IS NOT NULL
                    AND COL_LENGTH(N'[StockMovements]', N'StockItemId') IS NOT NULL
                BEGIN
                    UPDATE [movement]
                    SET [StockItemId] = [stockItem].[Id]
                    FROM [StockMovements] [movement]
                    INNER JOIN [StockItems] [stockItem]
                        ON [stockItem].[ProductId] = [movement].[ProductId]
                        AND [stockItem].[WarehouseId] = [movement].[WarehouseId]
                    WHERE [movement].[StockItemId] IS NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
