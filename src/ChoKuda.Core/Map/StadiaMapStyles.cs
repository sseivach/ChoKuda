namespace ChoKuda.Core.Map;

public static class StadiaMapStyles
{
    public const string DefaultStyleId = "osm_bright";

    private const string StadiaAttribution =
        "&copy; <a href=\"https://stadiamaps.com/\" target=\"_blank\">Stadia Maps</a> " +
        "&copy; <a href=\"https://openmaptiles.org/\" target=\"_blank\">OpenMapTiles</a> " +
        "&copy; <a href=\"https://www.openstreetmap.org/copyright\" target=\"_blank\">OpenStreetMap contributors</a>";

    private const string StamenAttribution =
        "&copy; <a href=\"https://stadiamaps.com/\" target=\"_blank\">Stadia Maps</a> " +
        "&copy; <a href=\"https://stamen.com/\" target=\"_blank\">Stamen Design</a> " +
        "&copy; <a href=\"https://openmaptiles.org/\" target=\"_blank\">OpenMapTiles</a> " +
        "&copy; <a href=\"https://www.openstreetmap.org/copyright\" target=\"_blank\">OpenStreetMap contributors</a>";

    private const string StamenWatercolorAttribution =
        "&copy; <a href=\"https://stadiamaps.com/\" target=\"_blank\">Stadia Maps</a> " +
        "&copy; <a href=\"https://stamen.com/\" target=\"_blank\">Stamen Design</a> " +
        "&copy; <a href=\"https://www.openstreetmap.org/copyright\" target=\"_blank\">OpenStreetMap contributors</a>";

    private const string SatelliteAttribution =
        "&copy; CNES, Distribution Airbus DS, &copy; Airbus DS, &copy; PlanetObserver (Contains Copernicus Data) | " +
        StadiaAttribution;

    private static readonly IReadOnlyDictionary<string, StadiaMapStyle> ById;

    static StadiaMapStyles()
    {
        All =
        [
            new("osm_bright", "OSM Bright", "Base maps", "png", true, 20, StadiaAttribution),
            new("outdoors", "Stadia Outdoors", "Base maps", "png", true, 20, StadiaAttribution),
            new("alidade_smooth", "Alidade Smooth", "Base maps", "png", true, 20, StadiaAttribution),
            new("alidade_smooth_dark", "Alidade Smooth Dark", "Base maps", "png", true, 20, StadiaAttribution),
            new("alidade_satellite", "Alidade Satellite", "Base maps", "jpg", true, 20, SatelliteAttribution),

            new("stamen_toner", "Stamen Toner", "Stamen styles", "png", true, 20, StamenAttribution),
            new("stamen_toner_lite", "Stamen Toner Lite", "Stamen styles", "png", true, 20, StamenAttribution),
            new("stamen_toner_dark", "Stamen Toner Dark", "Stamen styles", "png", true, 20, StamenAttribution),
            new("stamen_toner_blacklite", "Stamen Toner Blacklite", "Stamen styles", "png", true, 20, StamenAttribution),
            new("stamen_terrain", "Stamen Terrain", "Stamen styles", "png", true, 20, StamenAttribution),
            new("stamen_watercolor", "Stamen Watercolor", "Stamen styles", "jpg", false, 16, StamenWatercolorAttribution),

            new("stamen_toner_background", "Stamen Toner Background (layer group)", "Stamen layer groups", "png", true, 20, StamenAttribution),
            new("stamen_toner_lines", "Stamen Toner Lines (layer group)", "Stamen layer groups", "png", true, 20, StamenAttribution),
            new("stamen_toner_labels", "Stamen Toner Labels (layer group)", "Stamen layer groups", "png", true, 20, StamenAttribution),
            new("stamen_terrain_background", "Stamen Terrain Background (layer group)", "Stamen layer groups", "png", true, 20, StamenAttribution),
            new("stamen_terrain_lines", "Stamen Terrain Lines (layer group)", "Stamen layer groups", "png", true, 20, StamenAttribution),
            new("stamen_terrain_labels", "Stamen Terrain Labels (layer group)", "Stamen layer groups", "png", true, 20, StamenAttribution),
        ];

        ById = All.ToDictionary(style => style.Id, StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<StadiaMapStyle> All { get; }

    public static StadiaMapStyle Default =>
        Get(DefaultStyleId);

    public static StadiaMapStyle Get(string? styleId) =>
        !string.IsNullOrWhiteSpace(styleId) && ById.TryGetValue(styleId.Trim(), out var style)
            ? style
            : ById[DefaultStyleId];

    public static bool IsSupported(string? styleId) =>
        !string.IsNullOrWhiteSpace(styleId) && ById.ContainsKey(styleId.Trim());

    public static string NormalizeStyleId(string? styleId) =>
        Get(styleId).Id;
}
