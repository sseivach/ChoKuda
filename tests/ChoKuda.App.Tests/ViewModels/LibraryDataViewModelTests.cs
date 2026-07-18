using ChoKuda.App.ViewModels;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class LibraryDataViewModelTests
{
    [Fact]
    public void SetLoadedBuildsTagsRemovesMissingFiltersAndProjectsMapPoints()
    {
        var keptCollectionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var removedCollectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var point = CreatePoint("Waterfall", "#safe #waterfall", keptCollectionId);
        var data = new LibraryDataViewModel();
        var search = new SearchViewModel();
        var filters = new FilterViewModel();
        filters.ToggleCollection(keptCollectionId, true);
        filters.ToggleCollection(removedCollectionId, true);
        filters.ToggleTag("#safe", true);
        filters.ToggleTag("#gone", true);

        data.SetLoaded(
            [point],
            [CreateCollection(keptCollectionId, "Arizona", "sun-fill", "#ff0000")],
            search,
            filters);

        Assert.Equal([point], data.AllPoints);
        Assert.Equal([point], data.FilteredPoints);
        Assert.Equal(["#safe", "#waterfall"], data.TagIndex);
        Assert.Equal([keptCollectionId], filters.SelectedCollectionIds);
        Assert.Equal(["#safe"], filters.SelectedTags);
        var mapPoint = Assert.Single(data.MapPoints);
        Assert.Equal("sun-fill", mapPoint.PinIconId);
        Assert.Equal("#ff0000", mapPoint.PinColor);
        Assert.Equal("#ffffff", mapPoint.PinIconColor);
    }

    [Fact]
    public void RebuildProjectionAppliesCurrentSearchFiltersAndCollectionPriority()
    {
        var primaryCollectionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var priorityCollectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var matchingPoint = CreatePoint("Waterfall", "#safe", primaryCollectionId, priorityCollectionId);
        matchingPoint.PrimaryCollectionId = primaryCollectionId;
        var filteredPoint = CreatePoint("Desert", "#safe", primaryCollectionId);
        var data = new LibraryDataViewModel();
        var search = new SearchViewModel { Input = "water" };
        var filters = new FilterViewModel();
        data.SetLoaded(
            [filteredPoint, matchingPoint],
            [
                CreateCollection(primaryCollectionId, "Primary", "geo-alt-fill", "#111111"),
                CreateCollection(priorityCollectionId, "Priority", "star-fill", "#222222", "#00ffcc"),
            ],
            search,
            filters);

        Assert.True(search.Run());
        filters.ToggleCollection(priorityCollectionId, true);
        data.RebuildProjection(search, filters);

        Assert.Equal([matchingPoint], data.FilteredPoints);
        Assert.Equal([matchingPoint], data.SearchResults);
        var mapPoint = Assert.Single(data.MapPoints);
        Assert.Equal("star-fill", mapPoint.PinIconId);
        Assert.Equal("#222222", mapPoint.PinColor);
        Assert.Equal("#00ffcc", mapPoint.PinIconColor);
    }

    [Fact]
    public void SearchResultsSortFilteredPointsByTitleThenId()
    {
        var laterId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var earlierId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var data = new LibraryDataViewModel();
        var search = new SearchViewModel();
        var filters = new FilterViewModel();
        var zed = CreatePoint(laterId, "Zed", "#safe");
        var alphaLater = CreatePoint(laterId, "Alpha", "#safe");
        var alphaEarlier = CreatePoint(earlierId, "Alpha", "#safe");

        data.SetLoaded([zed, alphaLater, alphaEarlier], [], search, filters);

        Assert.Equal([alphaEarlier, alphaLater, zed], data.SearchResults);
    }

    [Fact]
    public void ClearResetsDataAndRemovesUnavailableFilters()
    {
        var collectionId = Guid.NewGuid();
        var data = new LibraryDataViewModel();
        var search = new SearchViewModel();
        var filters = new FilterViewModel();
        filters.ToggleCollection(collectionId, true);
        filters.ToggleTag("#safe", true);
        data.SetLoaded([CreatePoint("Saved", "#safe", collectionId)], [CreateCollection(collectionId, "Saved", "sun-fill", "#ff0000")], search, filters);

        data.Clear(search, filters);

        Assert.Empty(data.AllPoints);
        Assert.Empty(data.FilteredPoints);
        Assert.Empty(data.Collections);
        Assert.Empty(data.MapPoints);
        Assert.Empty(data.TagIndex);
        Assert.Empty(filters.SelectedCollectionIds);
        Assert.Empty(filters.SelectedTags);
    }

    private static PointDocument CreatePoint(
        string title,
        string tags,
        params Guid[] collectionIds) =>
        CreatePoint(Guid.NewGuid(), title, tags, collectionIds);

    private static PointDocument CreatePoint(
        Guid id,
        string title,
        string tags,
        params Guid[] collectionIds) =>
        new()
        {
            Id = id,
            Title = title,
            Latitude = 1,
            Longitude = 2,
            TagsText = tags,
            CollectionIds = collectionIds.ToList(),
        };

    private static CollectionDocument CreateCollection(
        Guid id,
        string name,
        string iconId,
        string color,
        string iconColor = "#ffffff") =>
        new()
        {
            Id = id,
            Name = name,
            IconId = iconId,
            Color = color,
            IconColor = iconColor,
        };
}
