namespace SGRRHH.Local.Shared;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorMessage => Error; // Alias para compatibilidad
    public string? Message { get; }

    protected Result(bool isSuccess, string? error, string? message = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public static Result Ok(string? message = null) => new(true, null, message);

    public static Result Fail(string error) => new(false, error);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, string? error, T? value, string? message = null)
        : base(isSuccess, error, message)
    {
        Value = value;
    }

    public static Result<T> Ok(T value, string? message = null) => new(true, null, value, message);

    public static new Result<T> Fail(string error) => new(false, error, default);
}

