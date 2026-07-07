using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class PointCollectionRulesTests
{
    private static readonly Guid ArizonaId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid SummerId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid MexicoId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void AddCollectionMakesAddedCollectionPrimary()
    {
        var primaryId = PointCollectionRules.AddCollection(
            [ArizonaId],
            ArizonaId,
            SummerId);

        Assert.Equal(SummerId, primaryId);
    }

    [Fact]
    public void RemoveCollectionReturnsNullWhenNoCollectionsRemain()
    {
        var primaryId = PointCollectionRules.RemoveCollection(
            [],
            ArizonaId,
            ArizonaId,
            CreateCollections());

        Assert.Null(primaryId);
    }

    [Fact]
    public void RemoveCollectionKeepsCurrentPrimaryWhenItWasNotRemoved()
    {
        var primaryId = PointCollectionRules.RemoveCollection(
            [ArizonaId, MexicoId],
            SummerId,
            ArizonaId,
            CreateCollections());

        Assert.Equal(ArizonaId, primaryId);
    }

    [Fact]
    public void RemoveCollectionChoosesByNameWhenCurrentPrimaryIsNull()
    {
        var primaryId = PointCollectionRules.RemoveCollection(
            [ArizonaId, MexicoId],
            SummerId,
            null,
            CreateCollections());

        Assert.Equal(ArizonaId, primaryId);
    }

    [Fact]
    public void RemoveCollectionChoosesByNameWhenCurrentPrimaryIsNotRemaining()
    {
        var primaryId = PointCollectionRules.RemoveCollection(
            [MexicoId],
            SummerId,
            ArizonaId,
            CreateCollections());

        Assert.Equal(MexicoId, primaryId);
    }

    [Fact]
    public void RemoveCollectionChoosesFirstRemainingCollectionByNameWhenPrimaryWasRemoved()
    {
        var primaryId = PointCollectionRules.RemoveCollection(
            [SummerId, MexicoId],
            ArizonaId,
            ArizonaId,
            CreateCollections());

        Assert.Equal(MexicoId, primaryId);
    }

    [Fact]
    public void RemoveCollectionReturnsNullWhenRemainingCollectionIsUnknown()
    {
        var unknownId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var primaryId = PointCollectionRules.RemoveCollection(
            [unknownId],
            ArizonaId,
            ArizonaId,
            CreateCollections());

        Assert.Null(primaryId);
    }

    private static IReadOnlyCollection<CollectionSummary> CreateCollections() =>
        [
            new CollectionSummary(ArizonaId, "Arizona"),
            new CollectionSummary(SummerId, "Summer 26"),
            new CollectionSummary(MexicoId, "Mexico"),
        ];
}
