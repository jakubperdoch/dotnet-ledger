namespace Ledger.Domain.Exceptions;

public sealed class InsufficientFundsException : Exception
{
    public InsufficientFundsException(Money requested, Money available)
        : base($"Insufficient funds: requested {requested}, available {available}.")
    {
        Requested = requested;
        Available = available;
    }

    public Money Requested { get; }
    public Money Available { get; }
}
