using Ledger.Domain.Exceptions;

namespace Ledger.Domain;

public sealed class TransferService
{
    private readonly IRepository<Account> _accounts;

    public TransferService(IRepository<Account> accounts)
    {
        _accounts = accounts;
    }

    public void Transfer(Guid fromId, Guid toId, Money amount, string? description = null)
    {
        if (fromId == toId)
        {
            throw new ArgumentException("Cannot transfer money to same account.", nameof(fromId));
        }

        Account from = _accounts.GetById(fromId) ?? throw new AccountNotFoundException(fromId);
        Account to = _accounts.GetById(toId) ?? throw new AccountNotFoundException(toId);


        if (from.Currency != to.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot transfer from account with currency {from.Currency} to account with currency {to.Currency}.");
        }

        // Withdraw runs first because it validates funds and amount; once it succeeds, Deposit cannot fail and leave the transfer half-applied.
        from.Withdraw(amount, description);
        to.Deposit(amount, description);
    }
}
