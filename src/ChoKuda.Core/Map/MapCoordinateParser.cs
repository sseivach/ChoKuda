using System.Globalization;

namespace ChoKuda.Core.Map;

public static class MapCoordinateParser
{
    private const string ExpectedFormatMessage =
        "Enter coordinates as latitude, longitude. Example: 40.104269854933186, -80.66645820627119.";

    public static MapCoordinateParseResult ParseDecimalPair(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return MapCoordinateParseResult.Failure(ExpectedFormatMessage);
        }

        var parts = input.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
        {
            return MapCoordinateParseResult.Failure(ExpectedFormatMessage);
        }

        if (!TryParseDecimal(parts[0], out var latitude) ||
            !TryParseDecimal(parts[1], out var longitude))
        {
            return MapCoordinateParseResult.Failure("Latitude and longitude must be decimal numbers with dot separators.");
        }

        if (latitude is < -90 or > 90)
        {
            return MapCoordinateParseResult.Failure("Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            return MapCoordinateParseResult.Failure("Longitude must be between -180 and 180.");
        }

        return MapCoordinateParseResult.Success(new MapCoordinate
        {
            Latitude = latitude,
            Longitude = longitude,
        });
    }

    private static bool TryParseDecimal(string value, out double number)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
        {
            return false;
        }

        return double.IsFinite(number);
    }
}
