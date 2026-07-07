using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.FileLibrary;

public sealed class LibrarySettingsTests
{
    [Fact]
    public void CreateDefaultReturnsCurrentSchemaAndStadiaProvider()
    {
        var settings = LibrarySettings.CreateDefault();

        Assert.Equal(1, settings.SchemaVersion);
        Assert.Equal("Stadia", settings.MapProvider);
    }
}

