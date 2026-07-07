using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.FileLibrary;

public sealed class StandardPathsTests
{
    [Fact]
    public void GetDefaultLibraryPathUsesDocumentsChoKudaLibrary()
    {
        var path = StandardPaths.GetDefaultLibraryPath(@"C:\Users\Sergei");

        Assert.Equal(@"C:\Users\Sergei\Documents\ChoKudaLibrary", path);
    }

    [Fact]
    public void GetDefaultLibraryPathRejectsBlankUserProfilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            StandardPaths.GetDefaultLibraryPath(""));

        Assert.Equal("userProfilePath", exception.ParamName);
    }

    [Fact]
    public void GetAppSettingsFilePathUsesAppDataChoKudaFolder()
    {
        var path = StandardPaths.GetAppSettingsFilePath(@"C:\Users\Sergei\AppData\Roaming");

        Assert.Equal(@"C:\Users\Sergei\AppData\Roaming\ChoKuda\appsettings.json", path);
    }

    [Fact]
    public void GetAppSettingsFilePathRejectsBlankAppDataPath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            StandardPaths.GetAppSettingsFilePath(" "));

        Assert.Equal("appDataPath", exception.ParamName);
    }
}

