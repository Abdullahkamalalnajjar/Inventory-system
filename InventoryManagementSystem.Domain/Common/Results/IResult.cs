namespace InventoryManagementSystem.Domain.Common.Results;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsError { get; }
}
