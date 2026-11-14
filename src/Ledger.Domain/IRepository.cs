namespace Ledger.Domain;

public interface IRepository<T> where T : IEntity
{
    T? GetById(Guid id);
    void Add(T entity);
    IReadOnlyList<T> GetAll();
}
