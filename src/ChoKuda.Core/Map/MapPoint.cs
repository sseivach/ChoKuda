namespace ChoKuda.Core.Map;

public sealed record MapPoint(
    Guid Id,
    string Title,
    double Latitude,
    double Longitude,
    string PinIconId,
    string PinColor,
    string PinIconColor);
