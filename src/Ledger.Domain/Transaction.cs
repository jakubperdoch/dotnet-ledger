namespace Ledger.Domain;

public sealed record Transaction
{
    public Transaction(Guid id, TransactionType type, Money amount, DateTimeOffset createdAt,
        string? description = null)
    {
        // Transactions can be only in positive numbers. TransactionType defines type of transaction
        if (amount.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than or equal to zero.", nameof(amount));
        }

        Id = id;
        CreatedAt = createdAt;
        Description = description;
        Type = type;
        Amount = amount;
    }

    public Money Amount { get; }
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; }
    public string? Description { get; }
    public TransactionType Type { get; }
}
