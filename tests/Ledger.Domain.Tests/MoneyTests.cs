namespace Ledger.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        Money money = Money.Zero(Currency.EUR);

        Assert.Equal(0m, money.Amount);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(0, 0, 0)]
    [InlineData(10.5, 0.25, 10.75)]
    public void Add_SameCurrency_ReturnsSum(decimal a, decimal b, decimal expected)
    {
        Money result = new Money(a, Currency.USD) + new Money(b, Currency.USD);

        Assert.Equal(expected, result.Amount);
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        Money usd = new(1m, Currency.USD);
        Money eur = new(1m, Currency.EUR);

        Assert.Throws<InvalidOperationException>(() => usd + eur);
    }

    [Theory]
    [InlineData(1, 2, -1)]
    [InlineData(0, 0, 0)]
    [InlineData(10.5, 0.25, 10.25)]
    public void Subtract_SameCurrency_ReturnsDifference(decimal a, decimal b, decimal expected)
    {
        Money result = new Money(a, Currency.USD) - new Money(b, Currency.USD);

        Assert.Equal(expected, result.Amount);
    }

    [Fact]
    public void Subtract_DifferentCurrency_Throws()
    {
        Money usd = new(1m, Currency.USD);
        Money eur = new(1m, Currency.EUR);

        Assert.Throws<InvalidOperationException>(() => usd - eur);
    }

    [Fact]
    public void Equals_SameCurrency_ReturnsTrue()
    {
        Money money = new(10.5m, Currency.USD);
        Money money2 = new(10.5m, Currency.USD);

        Assert.True(money == money2);
    }

    [Fact]
    public void ToString_ReturnsAmountAndCurrency()
    {
        Money money = new(10.5m, Currency.USD);

        Assert.Equal("10.50 USD", money.ToString());
    }

    [Theory]
    [InlineData(1.233)]
    [InlineData(2.2345)]
    [InlineData(10.234567)]
    public void Constructor_MoreThanTwoDecimalPlaces_Throws(decimal amount)
    {
        Assert.Throws<ArgumentException>(() => new Money(amount, Currency.USD));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(2.23)]
    [InlineData(10.23)]
    public void Constructor_UpToTwoDecimalPlaces_StoresAmount(decimal amount)
    {
        Money money = new(amount, Currency.USD);

        Assert.Equal(amount, money.Amount);
    }
}
