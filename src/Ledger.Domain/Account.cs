using Ledger.Domain.Exceptions;

namespace Ledger.Domain;

public sealed class Account : IEntity
{
    private readonly TimeProvider _timeProvider;
    private readonly List<Transaction> _transactions = [];

    public Account(string owner, Currency currency, TimeProvider? timeProvider = null)
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
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    public string Owner { get; }
    public Currency Currency { get; }

    public Money Balance => _transactions.Aggregate(Money.Zero(Currency), (acc, tx) => tx.Type switch
    {
        TransactionType.Deposit => acc + tx.Amount,
        TransactionType.Withdrawal => acc - tx.Amount,
        _ => throw new InvalidOperationException($"Unknown transaction type: {tx.Type}")
    });

    public Guid Id { get; }

    private void EnsureAccountCurrency(Money amount)
    {
        if (amount.Currency != Currency)
        {
            throw new InvalidOperationException(
                $"Cannot use {amount.Currency} with a {Currency} account.");
        }
    }

    public void Deposit(Money amount, string? description = null)
    {
        EnsureAccountCurrency(amount);

        Transaction transaction = new(TransactionType.Deposit, amount, _timeProvider.GetUtcNow(), description);
        _transactions.Add(transaction);
    }

    public void Withdraw(Money amount, string? description = null)
    {
        EnsureAccountCurrency(amount);
        Money balance = Balance;

        if (amount.Amount > balance.Amount)
        {
            throw new InsufficientFundsException(amount, balance);
        }

        Transaction transaction = new(TransactionType.Withdrawal, amount, _timeProvider.GetUtcNow(), description);
        _transactions.Add(transaction);
    }

    public IReadOnlyList<Transaction> GetTransactions(DateTimeOffset from, DateTimeOffset to)
    {
        if (from > to)
        {
            throw new ArgumentOutOfRangeException(nameof(from), from, "'from' cannot be later than 'to'.");
        }

        return _transactions
            .Where(tx => tx.CreatedAt >= from && tx.CreatedAt <= to)
            .OrderByDescending(tx => tx.CreatedAt)
            .ToList();
    }

    public Money GetTotal(TransactionType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown transaction type.");
        }

        return _transactions
            .Where(tx => tx.Type == type)
            .Aggregate(Money.Zero(Currency), (acc, tx) => acc + tx.Amount);
    }
}
