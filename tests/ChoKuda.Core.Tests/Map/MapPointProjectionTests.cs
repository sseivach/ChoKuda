using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Map;

namespace ChoKuda.Core.Tests.Map;

public sealed class MapPointProjectionTests
{
    [Fact]
    public void FromPointDocumentsProjectsPointFieldsForMap()
    {
        var pointId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var point = new PointDocument
        {
            Id = pointId,
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
        };

        var mapPoint = Assert.Single(MapPointProjection.FromPointDocuments([point]));

        Assert.Equal(pointId, mapPoint.Id);
        Assert.Equal("Havasu Falls", mapPoint.Title);
        Assert.Equal(36.255, mapPoint.Latitude);
        Assert.Equal(-112.697, mapPoint.Longitude);
        Assert.Equal(PointDefaults.DefaultPinIconId, mapPoint.PinIconId);
        Assert.Equal(PointDefaults.DefaultPinColor, mapPoint.PinColor);
    }

    [Fact]
    public void FromPointDocumentsReturnsEmptyListForNoPoints()
    {
        var mapPoints = MapPointProjection.FromPointDocuments([]);

        Assert.Empty(mapPoints);
    }

    [Fact]
    public void MapCoordinateDefaultsToZeroCoordinates()
    {
        var coordinate = new MapCoordinate();

        Assert.Equal(0, coordinate.Latitude);
        Assert.Equal(0, coordinate.Longitude);
    }
}

