using ChoKuda.Core.Attachments;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.Attachments;

public sealed class AttachmentDeleteServiceTests
{
    [Fact]
    public void DefaultConstructorCreatesUsableService()
    {
        var service = new AttachmentDeleteService();

        Assert.NotNull(service);
    }

    [Fact]
    public void DeleteSavedAttachmentDeletesPhotoUpdatesJsonAndReturnsUpdatedPoint()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingAttachmentFileSystem();
        var service = new AttachmentDeleteService(fileSystem, fileLibrary);
        var point = CreatePoint();
        point.Photos = ["photo.jpg", "other.jpg"];
        fileLibrary.SavePoint(paths, point);
        File.WriteAllText(Path.Combine(paths.PhotosPath, "photo.jpg"), "photo");

        var result = service.DeleteSavedAttachment(paths, point, AttachmentKind.Photo, "photo.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal(["other.jpg"], result.Point.Photos);
        Assert.Equal(point.Files, result.Point.Files);
        Assert.False(File.Exists(Path.Combine(paths.PhotosPath, "photo.jpg")));
        var savedPoint = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(point.Id));
        Assert.Equal(["other.jpg"], savedPoint.Photos);
    }

    [Fact]
    public void DeleteSavedAttachmentDeletesFileUpdatesJsonAndReturnsUpdatedPoint()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingAttachmentFileSystem();
        var service = new AttachmentDeleteService(fileSystem, fileLibrary);
        var point = CreatePoint();
        point.Files = ["guide.pdf", "other.pdf"];
        fileLibrary.SavePoint(paths, point);
        File.WriteAllText(Path.Combine(paths.FilesPath, "guide.pdf"), "file");

        var result = service.DeleteSavedAttachment(paths, point, AttachmentKind.File, "guide.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal(["other.pdf"], result.Point.Files);
        Assert.False(File.Exists(Path.Combine(paths.FilesPath, "guide.pdf")));
        var savedPoint = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(point.Id));
        Assert.Equal(["other.pdf"], savedPoint.Files);
    }

    [Fact]
    public void DeleteSavedAttachmentTreatsMissingPhysicalFileAsAlreadyDeleted()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = new AttachmentDeleteService(new RecordingAttachmentFileSystem(), fileLibrary);
        var point = CreatePoint();
        point.Files = ["missing.pdf"];
        fileLibrary.SavePoint(paths, point);

        var result = service.DeleteSavedAttachment(paths, point, AttachmentKind.File, "missing.pdf");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Point.Files);
    }

    [Fact]
    public void DeleteSavedAttachmentRejectsEmptyAndInvalidStoredNames()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var service = new AttachmentDeleteService(new RecordingAttachmentFileSystem(), new FileLibraryService());
        var point = CreatePoint();

        var emptyResult = service.DeleteSavedAttachment(paths, point, AttachmentKind.File, " ");
        var invalidResult = service.DeleteSavedAttachment(paths, point, AttachmentKind.File, "../guide.pdf");

        Assert.False(emptyResult.IsSuccess);
        Assert.Contains("Attachment file name is required.", emptyResult.Errors);
        Assert.False(invalidResult.IsSuccess);
        Assert.Contains("Attachment file name is invalid: ../guide.pdf", invalidResult.Errors);
    }

    [Fact]
    public void DeleteSavedAttachmentKeepsJsonWhenPhysicalDeleteFails()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingAttachmentFileSystem();
        var service = new AttachmentDeleteService(fileSystem, fileLibrary);
        var point = CreatePoint();
        point.Photos = ["photo.jpg"];
        fileLibrary.SavePoint(paths, point);
        var photoPath = Path.Combine(paths.PhotosPath, "photo.jpg");
        File.WriteAllText(photoPath, "photo");
        fileSystem.ThrowOnDeletePaths.Add(photoPath);

        var result = service.DeleteSavedAttachment(paths, point, AttachmentKind.Photo, "photo.jpg");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.StartsWith("Attachment could not be deleted:", StringComparison.Ordinal));
        Assert.Equal(["photo.jpg"], result.Point.Photos);
        var savedPoint = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(point.Id));
        Assert.Equal(["photo.jpg"], savedPoint.Photos);
    }

    [Fact]
    public void AttachmentDeleteResultFactoriesCreateExpectedResults()
    {
        var point = CreatePoint();

        var success = AttachmentDeleteResult.Success(point);
        var failure = AttachmentDeleteResult.Failure(point, ["failed"]);

        Assert.True(success.IsSuccess);
        Assert.Same(point, success.Point);
        Assert.Empty(success.Errors);
        Assert.False(failure.IsSuccess);
        Assert.Equal(["failed"], failure.Errors);
    }

    private static PointDocument CreatePoint() =>
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            Photos = [],
            Files = [],
        };

    private sealed class RecordingAttachmentFileSystem : IAttachmentFileSystem
    {
        public HashSet<string> ThrowOnDeletePaths { get; } = [];

        public bool FileExists(string filePath) =>
            File.Exists(filePath);

        public void CopyFile(string sourcePath, string destinationPath) =>
            File.Copy(sourcePath, destinationPath);

        public void DeleteFile(string filePath)
        {
            if (ThrowOnDeletePaths.Contains(filePath))
            {
                throw new IOException("Delete failed.");
            }

            File.Delete(filePath);
        }
    }
}
