namespace ReserveFlow.Shared.Kernel.Results;

public class AppResult
{
    protected AppResult(bool isSuccess, ResultError error)
    {
        if (isSuccess && error != ResultError.None)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!isSuccess && error == ResultError.None)
        {
            throw new InvalidOperationException("A failed result must contain an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ResultError Error { get; }

    public static AppResult Success() => new(true, ResultError.None);

    public static AppResult<T> Success<T>(T value) => new(value);

    public static AppResult Failure(ResultError error) => new(false, error);

    public static AppResult<T> Failure<T>(ResultError error) => new(error);
}
