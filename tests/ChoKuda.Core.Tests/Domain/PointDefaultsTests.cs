using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class PointDefaultsTests
{
    [Fact]
    public void DefaultsMatchMvpRules()
    {
        Assert.Equal("New point", PointDefaults.DefaultTitle);
        Assert.Equal("geo-alt-fill", PointDefaults.DefaultPinIconId);
        Assert.Equal("#2f75b5", PointDefaults.DefaultPinColor);
        Assert.Equal("#ffffff", PointDefaults.DefaultPinIconColor);
    }
}
