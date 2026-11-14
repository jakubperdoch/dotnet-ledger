using Ledger.Domain.Exceptions;

namespace Ledger.Domain.Tests;

public class TransferServiceTests
{
    private readonly TransferService _sut = new();

    [Fact]
    public void Transfer_ValidInput_MovesMoneyBetweenAccounts()
    {
        Account from = new("Alice", Currency.EUR);
        Account to = new("Bob", Currency.EUR);
        from.Deposit(new Money(200m, Currency.EUR));

        _sut.Transfer(from, to, new Money(75m, Currency.EUR), "Rent");

        Assert.Equal(new Money(125m, Currency.EUR), from.Balance);
        Assert.Equal(new Money(75m, Currency.EUR), to.Balance);

        Transaction withdrawal = Assert.Single(from.Transactions, tx => tx.Type == TransactionType.Withdrawal);
        Assert.Equal(new Money(75m, Currency.EUR), withdrawal.Amount);
        Assert.Equal("Rent", withdrawal.Description);

        Transaction deposit = Assert.Single(to.Transactions);
        Assert.Equal(TransactionType.Deposit, deposit.Type);
        Assert.Equal(new Money(75m, Currency.EUR), deposit.Amount);
        Assert.Equal("Rent", deposit.Description);
    }

    [Fact]
    public void Transfer_SameAccount_Throws()
    {
        Account account = new("Alice", Currency.EUR);
        account.Deposit(new Money(100m, Currency.EUR));

        Assert.Throws<ArgumentException>(() =>
            _sut.Transfer(account, account, new Money(50m, Currency.EUR)));

        Assert.Equal(new Money(100m, Currency.EUR), account.Balance);
        Assert.Single(account.Transactions);
    }

    [Fact]
    public void Transfer_DifferentCurrencies_Throws()
    {
        Account from = new("Alice", Currency.EUR);
        Account to = new("Bob", Currency.USD);
        from.Deposit(new Money(100m, Currency.EUR));

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Transfer(from, to, new Money(50m, Currency.EUR)));

        Assert.Equal(new Money(100m, Currency.EUR), from.Balance);
        Assert.Equal(Money.Zero(Currency.USD), to.Balance);
        Assert.Empty(to.Transactions);
    }

    [Fact]
    public void Transfer_InsufficientFunds_Throws()
    {
        Account from = new("Alice", Currency.EUR);
        Account to = new("Bob", Currency.EUR);
        from.Deposit(new Money(50m, Currency.EUR));

        Assert.Throws<InsufficientFundsException>(() =>
            _sut.Transfer(from, to, new Money(100m, Currency.EUR)));

        Assert.Equal(new Money(50m, Currency.EUR), from.Balance);
        Assert.Equal(Money.Zero(Currency.EUR), to.Balance);
        Assert.Empty(to.Transactions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Transfer_NonPositiveAmount_Throws(decimal invalidAmount)
    {
        Account from = new("Alice", Currency.EUR);
        Account to = new("Bob", Currency.EUR);
        from.Deposit(new Money(100m, Currency.EUR));

        Assert.Throws<ArgumentException>(() =>
            _sut.Transfer(from, to, new Money(invalidAmount, Currency.EUR)));

        Assert.Equal(new Money(100m, Currency.EUR), from.Balance);
        Assert.Equal(Money.Zero(Currency.EUR), to.Balance);
        Assert.Empty(to.Transactions);
    }
}
