using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Map;

public static class MapPointProjection
{
    public static IReadOnlyList<MapPoint> FromPointDocuments(
        IEnumerable<PointDocument> points)
    {
        return points
            .Select(point => new MapPoint(
                point.Id,
                point.Title,
                point.Latitude,
                point.Longitude,
                PointDefaults.DefaultPinIconId,
                PointDefaults.DefaultPinColor))
            .ToArray();
    }
}

