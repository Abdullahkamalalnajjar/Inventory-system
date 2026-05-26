using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common;

namespace InventoryManagementSystem.Infrastructure.Data.Interceptors;

public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private const string SystemUserName = "system";
    private readonly IUser _currentUser;

    public AuditableEntityInterceptor(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        var userName = _currentUser.Id ?? SystemUserName;

        foreach (EntityEntry<AuditableEntity> entry in dbContext.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.CreatedBy = userName;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedUtc = utcNow;
                entry.Entity.LastModifiedBy = userName;
            }
        }
    }
}
