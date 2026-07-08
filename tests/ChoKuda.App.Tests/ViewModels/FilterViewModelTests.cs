using ChoKuda.App.ViewModels;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class FilterViewModelTests
{
    [Fact]
    public void TogglesCollectionsAndTagsAndAppliesModes()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var filters = new FilterViewModel();
        var points = new[]
        {
            CreatePoint("A", "#safe #waterfall", arizona, summer),
            CreatePoint("B", "#safe", arizona),
            CreatePoint("C", "#waterfall", summer),
        };

        filters.ToggleCollection(arizona, true);
        filters.ToggleCollection(summer, true);
        filters.SetCollectionMode(FilterMode.All);
        filters.ToggleTag("#safe", true);
        filters.ToggleTag("#waterfall", true);
        filters.SetTagMode(FilterMode.All);

        var result = filters.Apply(points);

        Assert.Equal(["A"], result.Select(point => point.Title));
    }

    [Fact]
    public void ResetsCollectionsAndTagsIndependently()
    {
        var collectionId = Guid.NewGuid();
        var filters = new FilterViewModel();
        filters.ToggleCollection(collectionId, true);
        filters.ToggleTag("#safe", true);

        filters.ResetCollections();

        Assert.Empty(filters.SelectedCollectionIds);
        Assert.Equal(["#safe"], filters.SelectedTags);

        filters.ResetTags();

        Assert.Empty(filters.SelectedTags);
    }

    [Fact]
    public void DropCollectionReordersSelectedCollections()
    {
        var first = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var second = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var third = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var filters = new FilterViewModel();
        filters.ToggleCollection(first, true);
        filters.ToggleCollection(second, true);
        filters.ToggleCollection(third, true);

        filters.StartCollectionDrag(third);
        var changed = filters.DropCollection(first);

        Assert.True(changed);
        Assert.Equal([third, first, second], filters.SelectedCollectionIds);
    }

    [Fact]
    public void RemoveMissingValuesKeepsOnlyAvailableFilters()
    {
        var keptCollection = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var removedCollection = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var filters = new FilterViewModel();
        filters.ToggleCollection(keptCollection, true);
        filters.ToggleCollection(removedCollection, true);
        filters.ToggleTag("#safe", true);
        filters.ToggleTag("#gone", true);

        filters.RemoveMissingValues([keptCollection], ["#safe"]);

        Assert.Equal([keptCollection], filters.SelectedCollectionIds);
        Assert.Equal(["#safe"], filters.SelectedTags);
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
