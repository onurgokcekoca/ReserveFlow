using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Shared.Kernel.ValueObjects;

public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static AppResult<Money> Create(decimal amount, string? currency)
    {
        if (amount <= 0)
        {
            return AppResult.Failure<Money>(ResultError.Validation("Money.InvalidAmount", "Amount must be greater than zero."));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return AppResult.Failure<Money>(ResultError.Validation("Money.InvalidCurrency", "Currency is required."));
        }

        if (currency.Length != 3)
        {
            return AppResult.Failure<Money>(ResultError.Validation("Money.InvalidCurrency", "Currency must be a 3-letter ISO code."));
        }

        return AppResult.Success(new Money(amount, currency.ToUpperInvariant()));
    }
}
