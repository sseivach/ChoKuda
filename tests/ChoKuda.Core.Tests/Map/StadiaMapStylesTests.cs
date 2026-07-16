using ChoKuda.Core.Map;

namespace ChoKuda.Core.Tests.Map;

public sealed class StadiaMapStylesTests
{
    [Fact]
    public void AllContainsExpectedRasterStylesAndLayerGroups()
    {
        Assert.Equal(17, StadiaMapStyles.All.Count);
        Assert.Contains(StadiaMapStyles.All, style => style.Id == "osm_bright");
        Assert.Contains(StadiaMapStyles.All, style => style.Id == "outdoors");
        Assert.Contains(StadiaMapStyles.All, style => style.Id == "alidade_satellite");
        Assert.Contains(StadiaMapStyles.All, style => style.Id == "stamen_watercolor");
        Assert.Contains(StadiaMapStyles.All, style => style.Id == "stamen_toner_labels");
        Assert.Contains(StadiaMapStyles.All, style => style.Id == "stamen_terrain_background");
    }

    [Fact]
    public void AllStyleIdsAreUnique()
    {
        var styleIds = StadiaMapStyles.All.Select(style => style.Id).ToArray();

        Assert.Equal(styleIds.Length, styleIds.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void DefaultUsesReadableOsmBrightStyle()
    {
        Assert.Equal("osm_bright", StadiaMapStyles.DefaultStyleId);
        Assert.Equal("OSM Bright", StadiaMapStyles.Default.DisplayName);
    }

    [Fact]
    public void GetFallsBackToDefaultForBlankOrUnknownStyle()
    {
        Assert.Equal(StadiaMapStyles.Default, StadiaMapStyles.Get(null));
        Assert.Equal(StadiaMapStyles.Default, StadiaMapStyles.Get("   "));
        Assert.Equal(StadiaMapStyles.Default, StadiaMapStyles.Get("unknown_style"));
    }

    [Fact]
    public void GetIgnoresCaseAndWhitespace()
    {
        var style = StadiaMapStyles.Get("  STAMEN_TERRAIN  ");

        Assert.Equal("stamen_terrain", style.Id);
    }

    [Fact]
    public void WatercolorUsesJpgWithoutRetinaAndLowerMaxZoom()
    {
        var style = StadiaMapStyles.Get("stamen_watercolor");

        Assert.Equal("jpg", style.Extension);
        Assert.False(style.SupportsRetina);
        Assert.Equal(16, style.MaxZoom);
    }

    [Fact]
    public void SatelliteUsesJpgWithImageryAttribution()
    {
        var style = StadiaMapStyles.Get("alidade_satellite");

        Assert.Equal("jpg", style.Extension);
        Assert.True(style.SupportsRetina);
        Assert.Contains("Airbus", style.AttributionHtml, StringComparison.Ordinal);
    }
}
