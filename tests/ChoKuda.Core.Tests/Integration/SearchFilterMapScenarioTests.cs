using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;
using ChoKuda.Core.Map;
using ChoKuda.Core.Search;

namespace ChoKuda.Core.Tests.Integration;

public sealed class SearchFilterMapScenarioTests
{
    [Fact]
    public void SearchFiltersAndMapProjectionWorkAsOneUserScenario()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var mexico = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var points = new[]
        {
            CreatePoint("Havasu Waterfall", "#waterfall #safe", arizona, arizona, summer),
            CreatePoint("Mexico Waterfall", "#waterfall", mexico, mexico),
            CreatePoint("Arizona Camp", "#safe", arizona, arizona),
        };

        var searched = SearchService.Apply(points, " waterfall ");
        var filtered = PointFilterService.Apply(
            searched,
            new PointFilterOptions([summer, arizona], FilterMode.Any, ["#safe"], FilterMode.All));
        var mapPoints = MapPointProjection.FromPointDocuments(
            filtered,
            [
                new CollectionMapStyle(arizona, "circle-fill", "#ff0000"),
                new CollectionMapStyle(summer, "sun-fill", "#ffaa00"),
                new CollectionMapStyle(mexico, "flag-fill", "#00aa00"),
            ],
            [summer, arizona]);

        var mapPoint = Assert.Single(mapPoints);
        Assert.Equal("Havasu Waterfall", mapPoint.Title);
        Assert.Equal("sun-fill", mapPoint.PinIconId);
        Assert.Equal("#ffaa00", mapPoint.PinColor);
    }

    [Fact]
    public void AllModesRequireEverySelectedCollectionAndTag()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var points = new[]
        {
            CreatePoint("A", "#safe #waterfall", arizona, arizona, summer),
            CreatePoint("B", "#safe #waterfall", arizona, arizona),
            CreatePoint("C", "#safe", summer, arizona, summer),
        };

        var filtered = PointFilterService.Apply(
            points,
            new PointFilterOptions([arizona, summer], FilterMode.All, ["#safe", "#waterfall"], FilterMode.All));

        Assert.Equal(["A"], filtered.Select(point => point.Title));
    }

    [Fact]
    public void EmptySearchQueryStillAllowsFiltersToControlVisiblePoints()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var mexico = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var points = new[]
        {
            CreatePoint("Arizona Safe", "#safe", arizona, arizona),
            CreatePoint("Mexico Safe", "#safe", mexico, mexico),
            CreatePoint("Arizona Hard", "#hard", arizona, arizona),
        };

        var searched = SearchService.Apply(points, "   ");
        var filtered = PointFilterService.Apply(
            searched,
            new PointFilterOptions([arizona], FilterMode.Any, ["#safe"], FilterMode.Any));

        Assert.Equal(["Arizona Safe"], filtered.Select(point => point.Title));
    }

    private static PointDocument CreatePoint(
        string title,
        string tags,
        Guid primaryCollectionId,
        params Guid[] collectionIds) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Latitude = 1,
            Longitude = 2,
            TagsText = tags,
            PrimaryCollectionId = primaryCollectionId,
            CollectionIds = collectionIds.ToList(),
        };
}
