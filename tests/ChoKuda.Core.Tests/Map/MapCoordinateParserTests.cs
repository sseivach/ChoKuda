using ChoKuda.Core.Map;

namespace ChoKuda.Core.Tests.Map;

public sealed class MapCoordinateParserTests
{
    [Theory]
    [InlineData("40.104269854933186, -80.66645820627119", 40.104269854933186, -80.66645820627119)]
    [InlineData("40.104269854933186,-80.66645820627119", 40.104269854933186, -80.66645820627119)]
    [InlineData("  40.1  ,  -80.2  ", 40.1, -80.2)]
    [InlineData("-90, -180", -90, -180)]
    [InlineData("90, 180", 90, 180)]
    public void ParseDecimalPairAcceptsGoogleMapsDecimalCoordinates(
        string input,
        double expectedLatitude,
        double expectedLongitude)
    {
        var result = MapCoordinateParser.ParseDecimalPair(input);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.Coordinate);
        Assert.Equal(expectedLatitude, result.Coordinate.Latitude);
        Assert.Equal(expectedLongitude, result.Coordinate.Longitude);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("40.1 -80.2")]
    [InlineData("40.1, -80.2, 10")]
    [InlineData("40.1,")]
    [InlineData(", -80.2")]
    [InlineData("40,1, -80,2")]
    public void ParseDecimalPairRejectsWrongShape(string input)
    {
        var result = MapCoordinateParser.ParseDecimalPair(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Coordinate);
        Assert.Equal(
            "Enter coordinates as latitude, longitude. Example: 40.104269854933186, -80.66645820627119.",
            result.ErrorMessage);
    }

    [Theory]
    [InlineData("north, -80.2")]
    [InlineData("40.1, west")]
    [InlineData("NaN, -80.2")]
    [InlineData("40.1, Infinity")]
    public void ParseDecimalPairRejectsNonDecimalNumbers(string input)
    {
        var result = MapCoordinateParser.ParseDecimalPair(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Coordinate);
        Assert.Equal("Latitude and longitude must be decimal numbers with dot separators.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("-90.00001, -80.2")]
    [InlineData("90.00001, -80.2")]
    public void ParseDecimalPairRejectsLatitudeOutsideWorldRange(string input)
    {
        var result = MapCoordinateParser.ParseDecimalPair(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Coordinate);
        Assert.Equal("Latitude must be between -90 and 90.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("40.1, -180.00001")]
    [InlineData("40.1, 180.00001")]
    public void ParseDecimalPairRejectsLongitudeOutsideWorldRange(string input)
    {
        var result = MapCoordinateParser.ParseDecimalPair(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Coordinate);
        Assert.Equal("Longitude must be between -180 and 180.", result.ErrorMessage);
    }
}
