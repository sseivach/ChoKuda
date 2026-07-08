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
    public void FromPointDocumentsUsesPrimaryCollectionStyle()
    {
        var collectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var point = new PointDocument
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            CollectionIds = [collectionId],
            PrimaryCollectionId = collectionId,
        };
        var style = new CollectionMapStyle(collectionId, "camera-fill", "#d94a38");

        var mapPoint = Assert.Single(MapPointProjection.FromPointDocuments([point], [style]));

        Assert.Equal("camera-fill", mapPoint.PinIconId);
        Assert.Equal("#d94a38", mapPoint.PinColor);
    }

    [Fact]
    public void FromPointDocumentsUsesCollectionPriorityBeforePrimaryCollection()
    {
        var arizonaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var point = new PointDocument
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            CollectionIds = [arizonaId, summerId],
            PrimaryCollectionId = arizonaId,
        };

        var mapPoint = Assert.Single(MapPointProjection.FromPointDocuments(
            [point],
            [
                new CollectionMapStyle(arizonaId, "geo-alt-fill", "#d94a38"),
                new CollectionMapStyle(summerId, "sun-fill", "#e0a21a"),
            ],
            [summerId, arizonaId]));

        Assert.Equal("sun-fill", mapPoint.PinIconId);
        Assert.Equal("#e0a21a", mapPoint.PinColor);
    }

    [Fact]
    public void FromPointDocumentsSkipsPriorityCollectionsWithoutMatchingStyle()
    {
        var arizonaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var missingStyleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var point = new PointDocument
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            CollectionIds = [missingStyleId, arizonaId],
            PrimaryCollectionId = arizonaId,
        };

        var mapPoint = Assert.Single(MapPointProjection.FromPointDocuments(
            [point],
            [
                new CollectionMapStyle(arizonaId, "geo-alt-fill", "#d94a38"),
                new CollectionMapStyle(summerId, "sun-fill", "#e0a21a"),
            ],
            [summerId, missingStyleId, arizonaId]));

        Assert.Equal("geo-alt-fill", mapPoint.PinIconId);
        Assert.Equal("#d94a38", mapPoint.PinColor);
    }

    [Fact]
    public void FromPointDocumentsFallsBackWhenPrimaryStyleIsMissing()
    {
        var point = new PointDocument
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            PrimaryCollectionId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        };

        var mapPoint = Assert.Single(MapPointProjection.FromPointDocuments([point]));

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
