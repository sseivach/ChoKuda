using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class CollectionNameComparerTests
{
    [Fact]
    public void CompareReturnsZeroForSameReference()
    {
        var collection = new CollectionSummary(Guid.NewGuid(), "Arizona");

        var result = CollectionNameComparer.Instance.Compare(collection, collection);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareSortsNullBeforeCollection()
    {
        var collection = new CollectionSummary(Guid.NewGuid(), "Arizona");

        Assert.True(CollectionNameComparer.Instance.Compare(null, collection) < 0);
        Assert.True(CollectionNameComparer.Instance.Compare(collection, null) > 0);
    }

    [Fact]
    public void CompareSortsByNameIgnoringCaseThenById()
    {
        var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var collections = new[]
        {
            new CollectionSummary(secondId, "arizona"),
            new CollectionSummary(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Mexico"),
            new CollectionSummary(firstId, "Arizona"),
        };

        var sorted = collections.Order(CollectionNameComparer.Instance).ToArray();

        Assert.Equal(firstId, sorted[0].Id);
        Assert.Equal(secondId, sorted[1].Id);
        Assert.Equal("Mexico", sorted[2].Name);
    }
}

