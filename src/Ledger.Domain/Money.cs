using System.Globalization;

namespace Ledger.Domain;

public readonly record struct Money
{
    public Money(decimal amount, Currency currency)
    {
        // Silent rounding would lead to financial loss when rounding to 2 decimal places
        if (decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentException("Amount must be rounded to 2 decimal places", nameof(amount));
        }

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public Currency Currency { get; }

    public static Money Zero(Currency currency)
    {
        return new Money(0, currency);
    }

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot combine money of different currencies: {a.Currency} and {b.Currency}");
        }
    }

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Amount:F2} {Currency}");
    }
}
