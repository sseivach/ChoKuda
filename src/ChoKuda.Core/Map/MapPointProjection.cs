using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Map;

public static class MapPointProjection
{
    public static IReadOnlyList<MapPoint> FromPointDocuments(
        IEnumerable<PointDocument> points,
        IEnumerable<CollectionMapStyle>? collectionStyles = null,
        IReadOnlyList<Guid>? collectionStylePriority = null)
    {
        var stylesByCollectionId = (collectionStyles ?? Array.Empty<CollectionMapStyle>())
            .ToDictionary(style => style.CollectionId);
        var priority = collectionStylePriority ?? Array.Empty<Guid>();

        return points
            .Select(point =>
            {
                var style = ChooseStyle(point, stylesByCollectionId, priority);

                return new MapPoint(
                    point.Id,
                    point.Title,
                    point.Latitude,
                    point.Longitude,
                    style.IconId,
                    style.Color,
                    style.IconColor);
            })
            .ToArray();
    }

    private static CollectionMapStyle ChooseStyle(
        PointDocument point,
        IReadOnlyDictionary<Guid, CollectionMapStyle> stylesByCollectionId,
        IReadOnlyList<Guid> collectionStylePriority)
    {
        foreach (var collectionId in collectionStylePriority)
        {
            if (point.CollectionIds.Contains(collectionId) &&
                stylesByCollectionId.TryGetValue(collectionId, out var priorityStyle))
            {
                return priorityStyle;
            }
        }

        if (point.PrimaryCollectionId.HasValue &&
            stylesByCollectionId.TryGetValue(point.PrimaryCollectionId.Value, out var primaryStyle))
        {
            return primaryStyle;
        }

        return new CollectionMapStyle(
            Guid.Empty,
            PointDefaults.DefaultPinIconId,
            PointDefaults.DefaultPinColor,
            PointDefaults.DefaultPinIconColor);
    }
}
