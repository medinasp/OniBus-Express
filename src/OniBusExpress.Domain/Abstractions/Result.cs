namespace OniBusExpress.Domain.Abstractions;

public sealed record Result
{
    public bool IsSuccess { get; }
    public DomainError? Error { get; }

    private Result(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(DomainError error) => new(false, error);

    public static implicit operator Result(DomainError error) => Failure(error);
}

public sealed record Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public DomainError? Error { get; }

    private Result(bool isSuccess, T? value, DomainError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(DomainError error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(DomainError error) => Failure(error);
}
