namespace Ledger.Domain.Exceptions;

public sealed class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid accountId)
        : base($"Account with this id: {accountId} was not found")
    {
        AccountId = accountId;
    }

    public Guid AccountId { get; }
}
