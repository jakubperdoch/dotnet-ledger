using Ledger.Domain;
using Ledger.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();
services.AddSingleton<IRepository<Account>, InMemoryRepository<Account>>();
services.AddTransient<TransferService>();

using ServiceProvider provider = services.BuildServiceProvider();

IRepository<Account> accountRepository = provider.GetRequiredService<IRepository<Account>>();
TransferService transferService = provider.GetRequiredService<TransferService>();

Account alice = new("Alice", Currency.EUR);
Account bob = new("Bob", Currency.EUR);

accountRepository.Add(alice);
accountRepository.Add(bob);

alice.Deposit(new Money(200m, Currency.EUR), "Initial deposit");

Console.WriteLine("=== Stav pred prevodom ===");
PrintBalances(accountRepository);

Money transferAmount = new(75m, Currency.EUR);
Console.WriteLine($"\nPrevádzam {transferAmount.Amount} {transferAmount.Currency} od {alice.Owner} pre {bob.Owner}...");
transferService.Transfer(alice.Id, bob.Id, transferAmount, "Rent");

Console.WriteLine("\n=== Stav po prevode ===");
PrintBalances(accountRepository);

static void PrintBalances(IRepository<Account> repository)
{
    foreach (Account account in repository.GetAll())
    {
        Console.WriteLine($"- {account.Owner} ({account.Id}): {account.Balance}");
    }
}
