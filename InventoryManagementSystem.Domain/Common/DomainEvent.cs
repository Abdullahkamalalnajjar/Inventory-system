namespace InventoryManagementSystem.Domain.Common;

public abstract class DomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; protected init; } = DateTimeOffset.UtcNow;
}
