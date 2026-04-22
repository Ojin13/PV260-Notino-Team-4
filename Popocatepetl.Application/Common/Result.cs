namespace Popocatepetl.Application.Common;

/// <summary>Wraps an operation outcome, carrying either a value or an error message.</summary>
public sealed class Result<T>
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>The value produced on success; null on failure.</summary>
    public T? Value { get; }

    /// <summary>The error message on failure; null on success.</summary>
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>Creates a successful result carrying value.</summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>Creates a failed result with the given error message.</summary>
    public static Result<T> Failure(string error) => new(false, default, error);
}
