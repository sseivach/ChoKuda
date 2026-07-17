using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.FileLibrary;

public sealed class AppSettingsServiceTests
{
    [Fact]
    public void ConstructorRejectsBlankFilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new AppSettingsService(""));

        Assert.Equal("filePath", exception.ParamName);
    }

    [Fact]
    public void LoadReturnsDefaultSettingsWhenFileDoesNotExist()
    {
        using var temp = TempDirectory.Create();
        var service = new AppSettingsService(Path.Combine(temp.Path, "appsettings.json"));

        var settings = service.Load();

        Assert.Null(settings.LibraryPath);
        Assert.Null(settings.StadiaApiKey);
        Assert.Null(settings.StadiaMapStyleId);
        Assert.False(settings.UseLargeMapLabels);
    }

    [Fact]
    public void LoadReturnsDefaultSettingsWhenJsonIsNull()
    {
        using var temp = TempDirectory.Create();
        var filePath = Path.Combine(temp.Path, "appsettings.json");
        File.WriteAllText(filePath, "null");
        var service = new AppSettingsService(filePath);

        var settings = service.Load();

        Assert.Null(settings.LibraryPath);
        Assert.Null(settings.StadiaApiKey);
        Assert.Null(settings.StadiaMapStyleId);
        Assert.False(settings.UseLargeMapLabels);
    }

    [Fact]
    public void SaveCreatesDirectoryAndRoundTripsSettings()
    {
        using var temp = TempDirectory.Create();
        var filePath = Path.Combine(temp.Path, "nested", "appsettings.json");
        var service = new AppSettingsService(filePath);

        service.Save(new AppSettings
        {
            LibraryPath = @"C:\ChoKudaLibrary",
            StadiaApiKey = "stadia-key",
            StadiaMapStyleId = "outdoors",
            UseLargeMapLabels = true,
        });

        var loaded = service.Load();
        Assert.Equal(@"C:\ChoKudaLibrary", loaded.LibraryPath);
        Assert.Equal("stadia-key", loaded.StadiaApiKey);
        Assert.Equal("outdoors", loaded.StadiaMapStyleId);
        Assert.True(loaded.UseLargeMapLabels);
    }

    [Fact]
    public void SaveCanWriteFileWithoutDirectoryComponent()
    {
        using var temp = TempDirectory.Create();
        var previousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(temp.Path);

        try
        {
            var service = new AppSettingsService("appsettings.json");
            service.Save(new AppSettings
            {
                LibraryPath = "library",
                StadiaApiKey = "key",
                StadiaMapStyleId = "stamen_terrain",
                UseLargeMapLabels = true,
            });

            var loaded = service.Load();
            Assert.Equal("library", loaded.LibraryPath);
            Assert.Equal("key", loaded.StadiaApiKey);
            Assert.Equal("stamen_terrain", loaded.StadiaMapStyleId);
            Assert.True(loaded.UseLargeMapLabels);
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }
}
