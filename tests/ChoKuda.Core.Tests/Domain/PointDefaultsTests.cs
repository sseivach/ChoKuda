using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class PointDefaultsTests
{
    [Fact]
    public void DefaultsMatchMvpRules()
    {
        Assert.Equal("Новая точка", PointDefaults.DefaultTitle);
        Assert.Equal("default-pin", PointDefaults.DefaultPinIconId);
        Assert.Equal("#808080", PointDefaults.DefaultPinColor);
    }
}

