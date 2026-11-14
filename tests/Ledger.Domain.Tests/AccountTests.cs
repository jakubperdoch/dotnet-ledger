using Ledger.Domain.Exceptions;

using Microsoft.Extensions.Time.Testing;

namespace Ledger.Domain.Tests;

public class AccountTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_BlankOwner_Throws(string owner)
    {
        Assert.Throws<ArgumentException>(() => new Account(owner, Currency.USD));
    }

    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        Account account = new("John Doe", Currency.USD);

        Assert.Equal("John Doe", account.Owner);
        Assert.Equal(Currency.USD, account.Currency);
        Assert.NotEqual(Guid.Empty, account.Id);
    }

    [Fact]
    public void Deposit_ValidAmount_AddsDepositTransaction()
    {
        Account account = new("John Doe", Currency.EUR);
        Money amount = new(100m, Currency.EUR);

        account.Deposit(amount, "Salary");

        Transaction transaction = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal("Salary", transaction.Description);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    public void Deposit_SameCurrency_IncreasesBalance(decimal amount)
    {
        Account account = new("John Doe", Currency.EUR);

        account.Deposit(new Money(amount, Currency.EUR));

        Assert.Equal(new Money(amount, Currency.EUR), account.Balance);
    }

    [Fact]
    public void Deposit_MultipleTimes_SumsBalance()
    {
        Account account = new("John Doe", Currency.EUR);

        account.Deposit(new Money(100m, Currency.EUR));
        account.Deposit(new Money(50m, Currency.EUR));

        Assert.Equal(new Money(150m, Currency.EUR), account.Balance);
    }

    [Fact]
    public void Balance_NewAccount_IsZero()
    {
        Account account = new("John Doe", Currency.USD);

        Assert.Equal(Money.Zero(Currency.USD), account.Balance);
    }

    [Theory]
    [InlineData(100, Currency.EUR)]
    [InlineData(1000, Currency.CZK)]
    public void Deposit_DifferentCurrency_Throws(decimal amount, Currency currency)
    {
        Account account = new("John Doe", Currency.USD);

        Assert.Throws<InvalidOperationException>(() => account.Deposit(new Money(amount, currency)));
        Assert.Empty(account.Transactions);
    }

    [Theory]
    [InlineData(-200)]
    [InlineData(0)]
    public void Deposit_NonPositiveAmount_Throws(decimal amount)
    {
        Account account = new("John Doe", Currency.USD);

        Assert.Throws<ArgumentException>(() => account.Deposit(new Money(amount, Currency.USD)));
        Assert.Empty(account.Transactions);
    }

    [Fact]
    public void Constructor_UnknownCurrency_Throws()
    {
        Currency unknown = (Currency)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => new Account("John Doe", unknown));
    }

    [Fact]
    public void Withdraw_ValidAmount_AddsWithdrawalTransaction()
    {
        Account account = new("John Doe", Currency.EUR);
        Money deposit = new(100m, Currency.EUR);
        Money withdraw = new(70m, Currency.EUR);

        account.Deposit(deposit, "Salary");
        account.Withdraw(withdraw, "Grocery shopping");

        Assert.Equal(2, account.Transactions.Count);
        Transaction withdrawal = account.Transactions[^1];
        Assert.Equal(TransactionType.Withdrawal, withdrawal.Type);
        Assert.Equal(withdraw, withdrawal.Amount);
        Assert.Equal("Grocery shopping", withdrawal.Description);
    }

    [Fact]
    public void Withdraw_EntireBalance_LeavesZero()
    {
        Account account = new("John Doe", Currency.EUR);
        Money deposit = new(100m, Currency.EUR);
        Money withdraw = new(100m, Currency.EUR);

        account.Deposit(deposit, "Salary");
        account.Withdraw(withdraw, "Grocery shopping");

        Assert.Equal(Money.Zero(Currency.EUR), account.Balance);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        Account account = new("John Doe", Currency.EUR);
        Money deposit = new(100m, Currency.EUR);
        Money withdraw = new(70m, Currency.EUR);

        account.Deposit(deposit, "Salary");
        account.Withdraw(withdraw, "Grocery shopping");

        Assert.Equal(2, account.Transactions.Count);
        Transaction withdrawal = account.Transactions[^1];
        Assert.Equal(TransactionType.Withdrawal, withdrawal.Type);
        Assert.Equal(withdraw, withdrawal.Amount);
        Assert.Equal(30m, account.Balance.Amount);
    }

    [Fact]
    public void Withdraw_DifferentCurrency_Throws()
    {
        Account account = new("John Doe", Currency.EUR);
        Money deposit = new(100m, Currency.EUR);

        account.Deposit(deposit, "Salary");

        Assert.Throws<InvalidOperationException>(() =>
            account.Withdraw(new Money(70m, Currency.USD), "Grocery shopping"));
    }

    [Fact]
    public void Withdraw_InsufficientFunds_Throws()
    {
        Account account = new("John Doe", Currency.EUR);
        Money deposit = new(100m, Currency.EUR);

        account.Deposit(deposit, "Salary");

        InsufficientFundsException ex = Assert.Throws<InsufficientFundsException>(() =>
            account.Withdraw(new Money(150m, Currency.EUR), "Grocery shopping"));
        Assert.Equal(new Money(150m, Currency.EUR), ex.Requested);
        Assert.Equal(new Money(100m, Currency.EUR), ex.Available);
        Assert.Single(account.Transactions);
        Assert.Equal(new Money(100m, Currency.EUR), account.Balance);
    }

    [Fact]
    public void GetTotal_WithMultipleDeposits_ReturnsSumOfDeposits()
    {
        Account account = new("John Doe", Currency.EUR);

        for (int i = 0; i < 3; i++)
        {
            account.Deposit(new Money(100m, Currency.EUR), $"Deposit {i + 1}");
        }

        Money total = account.GetTotal(TransactionType.Deposit);
        Assert.Equal(new Money(300m, Currency.EUR), total);
    }

    [Fact]
    public void GetTotal_WithMultipleWithdrawals_ReturnsSumOfWithdrawals()
    {
        Account account = new("John Doe", Currency.EUR);
        account.Deposit(new Money(150m, Currency.EUR), "Deposit 1");

        for (int i = 0; i < 3; i++)
        {
            account.Withdraw(new Money(50m, Currency.EUR), $"Withdrawal {i + 1}");
        }

        Money withdraw = account.GetTotal(TransactionType.Withdrawal);
        Assert.Equal(new Money(150m, Currency.EUR), withdraw);
    }

    [Fact]
    public void GetTotal_UnknownTransactionType_Throws()
    {
        Account account = new("John Doe", Currency.EUR);
        TransactionType unknown = (TransactionType)99;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            account.GetTotal(unknown));
    }

    [Fact]
    public void GetTransactions_WithinDateRange_ReturnsMatchingTransactions()
    {
        FakeTimeProvider clock = new(new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero));
        Account account = new("John Doe", Currency.EUR, clock);

        account.Deposit(new Money(100m, Currency.EUR), "Before"); // 14.11. 12:00
        clock.Advance(TimeSpan.FromDays(2));
        account.Deposit(new Money(200m, Currency.EUR), "Inside"); // 16.11. 12:00
        clock.Advance(TimeSpan.FromDays(2));
        account.Withdraw(new Money(30m, Currency.EUR), "After"); // 18.11. 12:00

        IReadOnlyList<Transaction> result = account.GetTransactions(
            new DateTimeOffset(2026, 11, 15, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 17, 0, 0, 0, TimeSpan.Zero));

        Transaction single = Assert.Single(result);
        Assert.Equal(new Money(200m, Currency.EUR), single.Amount);
        Assert.Equal("Inside", single.Description);
    }

    [Fact]
    public void GetTransactions_OnExactBoundaryDates_IncludesBoundaryTransactions()
    {
        DateTimeOffset from = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        FakeTimeProvider clock = new(from);
        Account account = new("John Doe", Currency.EUR, clock);

        account.Deposit(new Money(100m, Currency.EUR), "Start"); // 14.11. 12:00
        clock.Advance(TimeSpan.FromDays(2));
        account.Withdraw(new Money(30m, Currency.EUR), "End"); // 16.11. 12:00
        DateTimeOffset to = clock.GetUtcNow();

        IReadOnlyList<Transaction> result = account.GetTransactions(from, to);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetTransactions_NoTransactionsInDateRange_ReturnsEmptyList()
    {
        FakeTimeProvider clock = new(new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero));
        Account account = new("John Doe", Currency.EUR, clock);

        account.Deposit(new Money(100m, Currency.EUR)); // 14.11. 12:00
        clock.Advance(TimeSpan.FromDays(2));
        account.Withdraw(new Money(30m, Currency.EUR)); // 16.11. 12:00

        IReadOnlyList<Transaction> result = account.GetTransactions(
            new DateTimeOffset(2026, 11, 20, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero));

        Assert.Empty(result);
    }

    [Fact]
    public void GetTransactions_EmptyAccount_ReturnsEmptyList()
    {
        Account account = new("John Doe", Currency.EUR);

        IReadOnlyList<Transaction> result = account.GetTransactions(
            new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero));

        Assert.Empty(result);
    }

    [Fact]
    public void GetTransactions_FromAfterTo_ThrowsArgumentOutOfRangeException()
    {
        Account account = new("John Doe", Currency.EUR);
        DateTimeOffset from = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset to = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(() => account.GetTransactions(from, to));
    }

    [Fact]
    public void GetTransactions_MultipleTransactions_ReturnsOrderedByCreatedAt()
    {
        FakeTimeProvider clock = new(new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero));
        Account account = new("John Doe", Currency.EUR, clock);

        account.Deposit(new Money(100m, Currency.EUR), "First"); // 20.11.
        clock.Advance(TimeSpan.FromDays(1));
        account.Deposit(new Money(200m, Currency.EUR), "Second"); // 21.11.
        clock.Advance(TimeSpan.FromDays(1));
        account.Withdraw(new Money(50m, Currency.EUR), "Third"); // 22.11.

        IReadOnlyList<Transaction> result = account.GetTransactions(
            new DateTimeOffset(2026, 11, 20, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 23, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(["Third", "Second", "First"], result.Select(tx => tx.Description));
    }
}
