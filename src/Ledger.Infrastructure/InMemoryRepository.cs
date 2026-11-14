using Ledger.Domain;

namespace Ledger.Infrastructure;

public sealed class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private readonly Dictionary<Guid, T> _items = [];

    public T? GetById(Guid id)
    {
        return _items.GetValueOrDefault(id);
    }

    public void Add(T item)
    {
        if (!_items.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException($"Entity with id: {item.Id} already exists.");
        }
    }

    public IReadOnlyList<T> GetAll()
    {
        return _items.Values.ToList();
    }
}
