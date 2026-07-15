using System.IO;
using System.Text.Json;

namespace ChoKuda.App.Services;

public sealed class BootstrapIconCatalog
{
    public static readonly IReadOnlyList<string> PreferredIconIds =
    [
        "geo-alt-fill", "pin-map-fill", "map-fill", "compass-fill", "camera-fill", "image-fill",
        "sun-fill", "moon-stars-fill", "cloud-sun-fill", "water", "tree-fill", "flower1",
        "fire", "umbrella-fill", "snow", "star-fill", "heart-fill", "flag-fill", "bookmark-fill",
        "signpost-fill", "binoculars-fill", "backpack-fill", "car-front-fill", "taxi-front-fill",
        "bus-front-fill", "train-front-fill", "airplane-fill", "bicycle", "scooter", "fuel-pump-fill",
        "ev-front-fill", "house-door-fill", "building-fill", "buildings-fill", "hospital-fill", "shop",
        "basket-fill", "cart-fill", "bag-fill", "cup-hot-fill", "cup-straw", "egg-fried",
        "cake2-fill", "fork-knife", "geo-fill", "crosshair", "bullseye", "diamond-fill",
        "circle-fill", "square-fill", "triangle-fill", "hexagon-fill", "pentagon-fill", "lightning-fill",
        "gem", "key-fill", "lock-fill", "shield-fill", "exclamation-triangle-fill", "info-circle-fill",
        "check-circle-fill", "x-circle-fill", "plus-circle-fill", "dash-circle-fill",
        "calendar-event-fill", "clock-fill", "alarm-fill", "wifi", "telephone-fill", "envelope-fill",
        "link-45deg", "paperclip", "file-earmark-fill", "file-earmark-pdf-fill", "journal-text",
        "book-fill", "music-note-beamed", "film", "controller", "palette-fill", "tools",
        "gear-fill", "person-walking", "cash-coin", "gift-fill", "trophy-fill", "rocket-takeoff-fill",
        "magic", "brightness-high-fill", "thermometer-sun", "droplet-fill", "wind", "layers-fill",
        "collection-fill", "tags-fill", "filter-circle-fill", "funnel-fill", "search", "grip-vertical",
        "trash-fill", "pencil-fill", "save-fill",
    ];

    private readonly string _catalogFilePath;

    public BootstrapIconCatalog()
        : this(Path.Combine(AppContext.BaseDirectory, "wwwroot", "data", "bootstrap-icons.json"))
    {
    }

    public BootstrapIconCatalog(string catalogFilePath)
    {
        _catalogFilePath = catalogFilePath;
    }

    public IReadOnlyList<string> LoadIconIds()
    {
        var loadedIconIds = TryLoadIconIds();
        var mergedIconIds = PreferredIconIds
            .Concat(loadedIconIds)
            .Where(IsValidIconId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return mergedIconIds.Length == 0
            ? PreferredIconIds
            : mergedIconIds;
    }

    private IReadOnlyList<string> TryLoadIconIds()
    {
        if (!File.Exists(_catalogFilePath))
        {
            return Array.Empty<string>();
        }

        try
        {
            var iconIds = JsonSerializer.Deserialize<string[]>(File.ReadAllText(_catalogFilePath));

            return iconIds ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    private static bool IsValidIconId(string iconId) =>
        !string.IsNullOrWhiteSpace(iconId) &&
        iconId.All(character =>
            character is >= 'a' and <= 'z' ||
            character is >= '0' and <= '9' ||
            character == '-');
}
