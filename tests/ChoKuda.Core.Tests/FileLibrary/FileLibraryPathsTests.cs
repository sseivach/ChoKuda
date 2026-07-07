using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.FileLibrary;

public sealed class FileLibraryPathsTests
{
    [Fact]
    public void ConstructorRejectsBlankRootPath()
    {
        var exception = Assert.Throws<ArgumentException>(() => new FileLibraryPaths(" "));

        Assert.Equal("rootPath", exception.ParamName);
    }

    [Fact]
    public void PathsAreDerivedFromRootPath()
    {
        var paths = new FileLibraryPaths(@"C:\ChoKudaLibrary");
        var pointId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var collectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        Assert.Equal(@"C:\ChoKudaLibrary", paths.RootPath);
        Assert.Equal(@"C:\ChoKudaLibrary\settings.json", paths.SettingsFilePath);
        Assert.Equal(@"C:\ChoKudaLibrary\points", paths.PointsPath);
        Assert.Equal(@"C:\ChoKudaLibrary\collections", paths.CollectionsPath);
        Assert.Equal(@"C:\ChoKudaLibrary\photos", paths.PhotosPath);
        Assert.Equal(@"C:\ChoKudaLibrary\files", paths.FilesPath);
        Assert.Equal(
            @"C:\ChoKudaLibrary\points\11111111-1111-1111-1111-111111111111.json",
            paths.GetPointFilePath(pointId));
        Assert.Equal(
            @"C:\ChoKudaLibrary\collections\22222222-2222-2222-2222-222222222222.json",
            paths.GetCollectionFilePath(collectionId));
    }
}

