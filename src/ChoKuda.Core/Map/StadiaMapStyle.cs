namespace ChoKuda.Core.Map;

public sealed record StadiaMapStyle(
    string Id,
    string DisplayName,
    string Group,
    string Extension,
    bool SupportsRetina,
    int MaxZoom,
    string AttributionHtml);
