namespace ChoKuda.Core.Map;

public sealed record MapCoordinateParseResult(
    bool IsSuccess,
    MapCoordinate? Coordinate,
    string? ErrorMessage)
{
    public static MapCoordinateParseResult Success(MapCoordinate coordinate) =>
        new(true, coordinate, null);

    public static MapCoordinateParseResult Failure(string errorMessage) =>
        new(false, null, errorMessage);
}
