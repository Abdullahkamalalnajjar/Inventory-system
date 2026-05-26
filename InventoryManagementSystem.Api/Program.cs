using InventoryManagementSystem.Api;
using InventoryManagementSystem.Application;
using InventoryManagementSystem.Api.Infrastructure;
using InventoryManagementSystem.Infrastructure;
using InventoryManagementSystem.Infrastructure.Data;
using InventoryManagementSystem.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.ExecuteSqlRawAsync("""
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

        var seeder = scope.ServiceProvider.GetRequiredService<AppDbContextSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Database initialization failed.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<RequestLogContextMiddleware>();
app.UseExceptionHandler();
app.UseCors("ApiCors");
app.UseOutputCache();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
