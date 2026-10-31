namespace Ledger.Domain;

public sealed class Account
{
    private readonly List<Transaction> _transactions = [];

    public Account(string owner, Currency currency)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            throw new ArgumentException("Owner cannot be null or whitespace.", nameof(owner));
        }

        if (!Enum.IsDefined(currency))
        {
            throw new ArgumentOutOfRangeException(nameof(currency), currency, "Unknown currency.");
        }


        Id = Guid.NewGuid();
        Owner = owner;
        Currency = currency;
    }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    public string Owner { get; }
    public Currency Currency { get; }
    public Guid Id { get; }

    public Money Balance => _transactions.Aggregate(Money.Zero(Currency), (acc, tx) => tx.Type switch
    {
        TransactionType.Deposit => acc + tx.Amount,
        TransactionType.Withdrawal => acc - tx.Amount,
        _ => throw new InvalidOperationException($"Unknown transaction type: {tx.Type}")
    });

    public void Deposit(Money amount, string? description = null)
    {
        if (amount.Currency != Currency)
        {
            throw new InvalidOperationException(
                $"Cannot deposit {amount.Currency} to a {Currency} account.");
        }

        Transaction transaction = new(TransactionType.Deposit, amount, description);
        _transactions.Add(transaction);
    }
}
