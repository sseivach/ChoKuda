using System.Text.Json;
using ChoKuda.App.Services;

namespace ChoKuda.App.Tests.Services;

public sealed class BootstrapIconCatalogTests
{
    [Fact]
    public void LoadIconIdsKeepsPreferredIconsFirstAndAddsJsonIcons()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);

        try
        {
            var catalogFilePath = Path.Combine(directoryPath, "bootstrap-icons.json");
            File.WriteAllText(catalogFilePath, JsonSerializer.Serialize(new[]
            {
                "zoom-in",
                "sun-fill",
                "bad icon",
                "camera-fill",
                "0-circle",
            }));
            var catalog = new BootstrapIconCatalog(catalogFilePath);

            var iconIds = catalog.LoadIconIds();

            Assert.Equal("geo-alt-fill", iconIds[0]);
            Assert.Contains("zoom-in", iconIds);
            Assert.Contains("0-circle", iconIds);
            Assert.DoesNotContain("bad icon", iconIds);
            Assert.Equal(iconIds.Count, iconIds.Distinct(StringComparer.Ordinal).Count());
            Assert.True(IndexOf(iconIds, "camera-fill") < IndexOf(iconIds, "zoom-in"));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public void LoadIconIdsFallsBackToPreferredIconsWhenJsonCannotBeLoaded()
    {
        var catalog = new BootstrapIconCatalog(Path.Combine("missing", "bootstrap-icons.json"));

        var iconIds = catalog.LoadIconIds();

        Assert.Equal(BootstrapIconCatalog.PreferredIconIds, iconIds);
    }

    private static int IndexOf(IReadOnlyList<string> iconIds, string iconId)
    {
        for (var index = 0; index < iconIds.Count; index++)
        {
            if (iconIds[index] == iconId)
            {
                return index;
            }
        }

        return -1;
    }
}
