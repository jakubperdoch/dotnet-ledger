using System.Diagnostics.CodeAnalysis;

namespace Ledger.Domain;

public sealed class TransferService
{
    [SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "Instance method by design; will be resolved via DI and gain dependencies.")]
    public void Transfer(Account from, Account to, Money amount, string? description = null)
    {
        if (from == to)
        {
            throw new ArgumentException("Cannot transfer money to same account.", nameof(from));
        }

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
