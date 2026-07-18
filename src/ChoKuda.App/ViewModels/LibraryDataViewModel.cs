using ChoKuda.Core.Domain;
using ChoKuda.Core.Collections;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;
using ChoKuda.Core.Map;
using TagIndexService = ChoKuda.Core.Filters.TagIndex;

namespace ChoKuda.App.ViewModels;

public sealed class LibraryDataViewModel
{
    public IReadOnlyList<PointDocument> AllPoints { get; private set; } = Array.Empty<PointDocument>();

    public IReadOnlyList<PointDocument> FilteredPoints { get; private set; } = Array.Empty<PointDocument>();

    public IReadOnlyList<CollectionDocument> Collections { get; private set; } = Array.Empty<CollectionDocument>();

    public IReadOnlyList<MapPoint> MapPoints { get; private set; } = Array.Empty<MapPoint>();

    public IReadOnlyList<string> TagIndex { get; private set; } = Array.Empty<string>();

    public IEnumerable<PointDocument> SearchResults =>
        FilteredPoints
            .OrderBy(point => point.Title, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(point => point.Id);

    public void Clear(
        SearchViewModel search,
        FilterViewModel filters)
    {
        AllPoints = Array.Empty<PointDocument>();
        Collections = Array.Empty<CollectionDocument>();
        TagIndex = Array.Empty<string>();
        filters.RemoveMissingValues(Array.Empty<Guid>(), Array.Empty<string>());
        RebuildProjection(search, filters);
    }

    public void SetLoaded(
        IReadOnlyList<PointDocument> points,
        IReadOnlyList<CollectionDocument> collections,
        SearchViewModel search,
        FilterViewModel filters)
    {
        AllPoints = points;
        Collections = collections;
        TagIndex = TagIndexService.Build(points);
        filters.RemoveMissingValues(
            Collections.Select(collection => collection.Id),
            TagIndex);
        RebuildProjection(search, filters);
    }

    public void RebuildProjection(
        SearchViewModel search,
        FilterViewModel filters)
    {
        var searchedPoints = search.Apply(AllPoints);

        FilteredPoints = filters.Apply(searchedPoints);
        MapPoints = MapPointProjection.FromPointDocuments(
            FilteredPoints,
            Collections.Select(collection => new CollectionMapStyle(
                collection.Id,
                IconOrDefault(collection.IconId),
                collection.Color,
                IconColorOrDefault(collection.IconColor))),
            filters.SelectedCollectionIds);
    }

    public PointDocument? FindPoint(Guid pointId) =>
        AllPoints.FirstOrDefault(point => point.Id == pointId);

    public CollectionDocument? FindCollection(Guid collectionId) =>
        Collections.FirstOrDefault(collection => collection.Id == collectionId);

    private static string IconOrDefault(string iconId) =>
        string.IsNullOrWhiteSpace(iconId)
            ? PointDefaults.DefaultPinIconId
            : iconId;

    private static string IconColorOrDefault(string iconColor) =>
        string.IsNullOrWhiteSpace(iconColor)
            ? CollectionColor.DefaultIconColor
            : iconColor;
}
