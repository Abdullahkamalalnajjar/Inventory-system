using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyProductQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL AND COL_LENGTH(N'[Products]', N'Quantity') IS NOT NULL
                BEGIN
                    DECLARE @defaultConstraintName sysname;

                    SELECT @defaultConstraintName = [dc].[name]
                    FROM [sys].[default_constraints] [dc]
                    INNER JOIN [sys].[columns] [c]
                        ON [c].[default_object_id] = [dc].[object_id]
                    INNER JOIN [sys].[tables] [t]
                        ON [t].[object_id] = [c].[object_id]
                    WHERE [t].[name] = N'Products'
                        AND [c].[name] = N'Quantity';

                    IF @defaultConstraintName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @defaultConstraintName + N']');
                    END

                    ALTER TABLE [Products] DROP COLUMN [Quantity];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL AND COL_LENGTH(N'[Products]', N'Quantity') IS NULL
                BEGIN
                    ALTER TABLE [Products] ADD [Quantity] int NOT NULL DEFAULT 0;
                END
                """);
        }
    }
}
