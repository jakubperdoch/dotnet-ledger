using Ledger.Domain;

namespace Ledger.Infrastructure.Tests;

public class InMemoryRepositoryTests
{
    private readonly InMemoryRepository<TestEntity> _sut = new();

    [Fact]
    public void Add_AndGetById_ReturnsSameEntity()
    {
        TestEntity entity = new(Guid.NewGuid());

        _sut.Add(entity);
        TestEntity? result = _sut.GetById(entity.Id);

        Assert.Same(entity, result);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        Guid unknownId = Guid.NewGuid();

        TestEntity? result = _sut.GetById(unknownId);

        Assert.Null(result);
    }

    [Fact]
    public void Add_DuplicateId_Throws()
    {
        Guid id = Guid.NewGuid();
        TestEntity first = new(id);
        TestEntity duplicate = new(id);
        _sut.Add(first);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            _sut.Add(duplicate));

        Assert.Contains(id.ToString(), ex.Message);
    }

    [Fact]
    public void GetAll_ReturnsAllAddedEntities()
    {
        TestEntity first = new(Guid.NewGuid());
        TestEntity second = new(Guid.NewGuid());

        _sut.Add(first);
        _sut.Add(second);

        IReadOnlyList<TestEntity> result = _sut.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Contains(first, result);
        Assert.Contains(second, result);
    }

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        IReadOnlyList<TestEntity> result = _sut.GetAll();

        Assert.Empty(result);
    }

    private sealed record TestEntity(Guid Id) : IEntity;
}
