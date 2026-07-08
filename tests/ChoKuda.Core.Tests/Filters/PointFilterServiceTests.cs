using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;

namespace ChoKuda.Core.Tests.Filters;

public sealed class PointFilterServiceTests
{
    [Fact]
    public void ApplyReturnsAllPointsWhenFiltersAreEmpty()
    {
        var points = new[]
        {
            CreatePoint("A", "#safe"),
            CreatePoint("B", "#hard"),
        };
        var options = new PointFilterOptions([], FilterMode.Any, [], FilterMode.Any);

        var result = PointFilterService.Apply(points, options);

        Assert.Equal(["A", "B"], result.Select(point => point.Title));
    }

    [Fact]
    public void MatchesCollectionsSupportsAnyMode()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var point = CreatePoint("A", "", arizona);

        Assert.True(PointFilterService.MatchesCollections(point, [arizona, summer], FilterMode.Any));
        Assert.False(PointFilterService.MatchesCollections(point, [summer], FilterMode.Any));
    }

    [Fact]
    public void MatchesCollectionsSupportsAllMode()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var point = CreatePoint("A", "", arizona, summer);

        Assert.True(PointFilterService.MatchesCollections(point, [arizona, summer], FilterMode.All));
        Assert.False(PointFilterService.MatchesCollections(point, [arizona, Guid.Parse("33333333-3333-3333-3333-333333333333")], FilterMode.All));
    }

    [Fact]
    public void MatchesTagsSupportsAnyModeAndNormalizesSelectedTags()
    {
        var point = CreatePoint("A", "#safe #rentcar");

        Assert.True(PointFilterService.MatchesTags(point, ["#SAFE", "missing"], FilterMode.Any));
        Assert.False(PointFilterService.MatchesTags(point, ["#hard"], FilterMode.Any));
    }

    [Fact]
    public void MatchesTagsSupportsAllMode()
    {
        var point = CreatePoint("A", "#safe #rentcar");

        Assert.True(PointFilterService.MatchesTags(point, ["#safe", "#rentcar"], FilterMode.All));
        Assert.False(PointFilterService.MatchesTags(point, ["#safe", "#hard"], FilterMode.All));
    }

    [Fact]
    public void ApplyCombinesCollectionsAndTagsWithAnd()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var points = new[]
        {
            CreatePoint("A", "#safe", arizona),
            CreatePoint("B", "#safe", summer),
            CreatePoint("C", "#hard", arizona),
        };
        var options = new PointFilterOptions([arizona], FilterMode.Any, ["#safe"], FilterMode.Any);

        var result = PointFilterService.Apply(points, options);

        Assert.Equal(["A"], result.Select(point => point.Title));
    }

    private static PointDocument CreatePoint(
        string title,
        string tags,
        params Guid[] collectionIds) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Latitude = 1,
            Longitude = 2,
            TagsText = tags,
            CollectionIds = collectionIds.ToList(),
        };
}
