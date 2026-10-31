namespace Ledger.Domain;

public sealed record Transaction
{
    public Transaction(TransactionType type, Money amount,
        string? description = null)
    {
        // Transactions can be only in positive numbers. TransactionType defines type of transaction
        if (amount.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        Description = description;
        Type = type;
        Amount = amount;
    }

    public Money Amount { get; }
    public Guid Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? Description { get; }
    public TransactionType Type { get; }
}
