using Ledger.Domain.Exceptions;
using Ledger.Infrastructure;

namespace Ledger.Domain.Tests;

public class TransferServiceTests
{
    private readonly InMemoryRepository<Account> _repository;
    private readonly TransferService _sut;

    public TransferServiceTests()
    {
        _repository = new InMemoryRepository<Account>();
        _sut = new TransferService(_repository);
    }

    [Fact]
    public void Transfer_ValidInput_MovesMoneyBetweenAccounts()
    {
        Account from = new("Alice", Currency.EUR);
        Account to = new("Bob", Currency.EUR);
        from.Deposit(new Money(200m, Currency.EUR));

        _repository.Add(from);
        _repository.Add(to);

        _sut.Transfer(from.Id, to.Id, new Money(75m, Currency.EUR), "Rent");

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

        _repository.Add(account);

        Assert.Throws<ArgumentException>(() =>
            _sut.Transfer(account.Id, account.Id, new Money(50m, Currency.EUR)));

        Assert.Equal(new Money(100m, Currency.EUR), account.Balance);
        Assert.Single(account.Transactions);
    }

    [Fact]
    public void Transfer_DifferentCurrencies_Throws()
    {
        Account from = new("Alice", Currency.EUR);
        Account to = new("Bob", Currency.USD);
        from.Deposit(new Money(100m, Currency.EUR));

        _repository.Add(from);
        _repository.Add(to);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Transfer(from.Id, to.Id, new Money(50m, Currency.EUR)));

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

        _repository.Add(from);
        _repository.Add(to);

        Assert.Throws<InsufficientFundsException>(() =>
            _sut.Transfer(from.Id, to.Id, new Money(100m, Currency.EUR)));

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

        _repository.Add(from);
        _repository.Add(to);

        Assert.Throws<ArgumentException>(() =>
            _sut.Transfer(from.Id, to.Id, new Money(invalidAmount, Currency.EUR)));

        Assert.Equal(new Money(100m, Currency.EUR), from.Balance);
        Assert.Equal(Money.Zero(Currency.EUR), to.Balance);
        Assert.Empty(to.Transactions);
    }

    [Fact]
    public void Transfer_UnknownFromAccount_Throws()
    {
        Guid unknownId = Guid.NewGuid();
        Account to = new("Bob", Currency.EUR);

        _repository.Add(to);

        AccountNotFoundException ex = Assert.Throws<AccountNotFoundException>(() =>
            _sut.Transfer(unknownId, to.Id, new Money(50m, Currency.EUR)));

        Assert.Equal(unknownId, ex.AccountId);
        Assert.Equal(Money.Zero(Currency.EUR), to.Balance);
    }

    [Fact]
    public void Transfer_UnknownToAccount_Throws()
    {
        Account from = new("Alice", Currency.EUR);
        from.Deposit(new Money(100m, Currency.EUR));
        Guid unknownId = Guid.NewGuid();

        _repository.Add(from);

        AccountNotFoundException ex = Assert.Throws<AccountNotFoundException>(() =>
            _sut.Transfer(from.Id, unknownId, new Money(50m, Currency.EUR)));

        Assert.Equal(unknownId, ex.AccountId);
        Assert.Equal(new Money(100m, Currency.EUR), from.Balance);
    }
}
