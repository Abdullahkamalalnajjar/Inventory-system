using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyRefreshTokenCreatedUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH(N'[RefreshTokens]', N'CreatedUtc') IS NOT NULL
                BEGIN
                    DECLARE @defaultConstraintName sysname;

                    SELECT @defaultConstraintName = [dc].[name]
                    FROM [sys].[default_constraints] [dc]
                    INNER JOIN [sys].[columns] [c]
                        ON [c].[default_object_id] = [dc].[object_id]
                    INNER JOIN [sys].[tables] [t]
                        ON [t].[object_id] = [c].[object_id]
                    WHERE [t].[name] = N'RefreshTokens'
                        AND [c].[name] = N'CreatedUtc';

                    IF @defaultConstraintName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [RefreshTokens] DROP CONSTRAINT [' + @defaultConstraintName + N']');
                    END

                    ALTER TABLE [RefreshTokens] DROP COLUMN [CreatedUtc];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH(N'[RefreshTokens]', N'CreatedUtc') IS NULL
                BEGIN
                    ALTER TABLE [RefreshTokens] ADD [CreatedUtc] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();
                END
                """);
        }
    }
}
