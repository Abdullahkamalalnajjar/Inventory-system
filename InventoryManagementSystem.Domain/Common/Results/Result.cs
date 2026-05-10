namespace InventoryManagementSystem.Domain.Common.Results;

public static class Result
{
    public static Success Success => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
}

public sealed class Result<TValue> : IResult
{
    private readonly TValue? value;
    private readonly List<Error> errors;

    private Result(TValue value)
    {
        this.value = value;
        errors = [];
        IsSuccess = true;
    }

    private Result(List<Error> errors)
    {
        this.errors = errors;
        value = default;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public bool IsError => !IsSuccess;

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public TValue Value => IsSuccess ? value! : default!;

    public List<Error> Errors => IsSuccess ? [] : errors;

    public TNextValue Match<TNextValue>(Func<TValue, TNextValue> onValue, Func<List<Error>, TNextValue> onError)
        => IsSuccess ? onValue(Value) : onError(Errors);

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(Error error) => new([error]);

    public static implicit operator Result<TValue>(List<Error> errors) => new(errors);
}

public readonly record struct Success;
public readonly record struct Deleted;
public readonly record struct Updated;
