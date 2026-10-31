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
}
