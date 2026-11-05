namespace ReserveFlow.Shared.Kernel.Results;

public sealed class AppResult<T> : AppResult
{
    private readonly T? _value;

    internal AppResult(T value) : base(true, ResultError.None)
    {
        _value = value;
    }

    internal AppResult(ResultError error) : base(false, error)
    {
    }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failed result cannot be accessed.");
}
