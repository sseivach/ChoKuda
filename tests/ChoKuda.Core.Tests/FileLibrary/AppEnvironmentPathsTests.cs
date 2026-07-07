using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.FileLibrary;

public sealed class AppEnvironmentPathsTests
{
    [Fact]
    public void ConstructorRejectsBlankAppDataPath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new AppEnvironmentPaths(" ", @"C:\Users\Sergei"));

        Assert.Equal("appDataPath", exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsBlankUserProfilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new AppEnvironmentPaths(@"C:\Users\Sergei\AppData\Roaming", ""));

        Assert.Equal("userProfilePath", exception.ParamName);
    }

    [Fact]
    public void PropertiesReturnConfiguredAndDerivedPaths()
    {
        var paths = new AppEnvironmentPaths(
            @"C:\Users\Sergei\AppData\Roaming",
            @"C:\Users\Sergei");

        Assert.Equal(@"C:\Users\Sergei\AppData\Roaming", paths.AppDataPath);
        Assert.Equal(@"C:\Users\Sergei", paths.UserProfilePath);
        Assert.Equal(
            @"C:\Users\Sergei\AppData\Roaming\ChoKuda\appsettings.json",
            paths.AppSettingsFilePath);
        Assert.Equal(
            @"C:\Users\Sergei\Documents\ChoKudaLibrary",
            paths.DefaultLibraryPath);
    }
}

