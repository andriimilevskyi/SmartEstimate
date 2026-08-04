namespace SmartEstimate.Shared.Primitives;

/// <summary>
/// Represents the outcome of an operation without coupling callers to exceptions.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("A failed result must contain an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result(false, error);
    }
}

/// <summary>
/// Represents the outcome of an operation that produces a value on success.
/// </summary>
/// <typeparam name="TValue">The operation's value type.</typeparam>
public sealed class Result<TValue> : Result
{
    private Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public TValue? Value { get; }

    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<TValue>(value, true, Error.None);
    }

    public static new Result<TValue> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<TValue>(default, false, error);
    }
}
