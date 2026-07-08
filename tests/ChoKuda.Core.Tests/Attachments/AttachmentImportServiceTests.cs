using ChoKuda.Core.Attachments;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.Attachments;

public sealed class AttachmentImportServiceTests
{
    [Fact]
    public void DefaultConstructorCreatesUsableService()
    {
        var service = new AttachmentImportService();

        Assert.NotNull(service);
    }

    [Fact]
    public void ImportPendingAttachmentsCopiesPhotosAndFilesAndUpdatesPoint()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var fileSystem = new RecordingAttachmentFileSystem();
        var service = new AttachmentImportService(fileSystem);
        var sourcePhoto = Path.Combine(temp.Path, "waterfall.jpg");
        var sourceFile = Path.Combine(temp.Path, "guide.pdf");
        File.WriteAllText(sourcePhoto, "photo");
        File.WriteAllText(sourceFile, "file");
        var point = CreatePoint();
        point.Photos = ["old.jpg"];
        point.Files = ["old.pdf"];

        var result = service.ImportPendingAttachments(
            paths,
            point,
            [
                new PendingAttachment(sourcePhoto, AttachmentKind.Photo),
                new PendingAttachment(sourceFile, AttachmentKind.File),
            ]);

        Assert.Empty(result.Errors);
        Assert.Equal(2, result.Point.Photos.Count);
        Assert.Equal(2, result.Point.Files.Count);
        Assert.Equal("old.jpg", result.Point.Photos[0]);
        Assert.Equal("old.pdf", result.Point.Files[0]);
        Assert.Matches(@"^waterfall__[0-9a-f]{32}\.jpg$", result.Point.Photos[1]);
        Assert.Matches(@"^guide__[0-9a-f]{32}\.pdf$", result.Point.Files[1]);
        Assert.Equal(2, fileSystem.CopiedFiles.Count);
        Assert.Contains(fileSystem.CopiedFiles, copy => copy.SourcePath == sourcePhoto && copy.DestinationPath.StartsWith(paths.PhotosPath, StringComparison.Ordinal));
        Assert.Contains(fileSystem.CopiedFiles, copy => copy.SourcePath == sourceFile && copy.DestinationPath.StartsWith(paths.FilesPath, StringComparison.Ordinal));
        Assert.Equal(["old.jpg"], point.Photos);
        Assert.Equal(["old.pdf"], point.Files);
    }

    [Fact]
    public void ImportPendingAttachmentsCollectsErrorsAndContinues()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var fileSystem = new RecordingAttachmentFileSystem();
        var service = new AttachmentImportService(fileSystem);
        var goodFile = Path.Combine(temp.Path, "good.pdf");
        var lockedFile = Path.Combine(temp.Path, "locked.pdf");
        File.WriteAllText(goodFile, "good");
        File.WriteAllText(lockedFile, "locked");
        fileSystem.ThrowOnCopySources.Add(lockedFile);

        var result = service.ImportPendingAttachments(
            paths,
            CreatePoint(),
            [
                new PendingAttachment("", AttachmentKind.File),
                new PendingAttachment(Path.Combine(temp.Path, "missing.pdf"), AttachmentKind.File),
                new PendingAttachment(lockedFile, AttachmentKind.File),
                new PendingAttachment(goodFile, AttachmentKind.File),
            ]);

        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.Message == "Source path is empty.");
        Assert.Contains(result.Errors, error => error.Message == "Source file was not found.");
        Assert.Contains(result.Errors, error => error.Message.StartsWith("File could not be copied:", StringComparison.Ordinal));
        Assert.Single(result.Point.Files);
        Assert.Matches(@"^good__[0-9a-f]{32}\.pdf$", result.Point.Files[0]);
    }

    private static PointDocument CreatePoint() =>
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
        };

    private sealed class RecordingAttachmentFileSystem : IAttachmentFileSystem
    {
        public List<(string SourcePath, string DestinationPath)> CopiedFiles { get; } = [];

        public HashSet<string> ThrowOnCopySources { get; } = [];

        public bool FileExists(string filePath) =>
            File.Exists(filePath);

        public void CopyFile(string sourcePath, string destinationPath)
        {
            if (ThrowOnCopySources.Contains(sourcePath))
            {
                throw new IOException("Copy failed.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath);
            CopiedFiles.Add((sourcePath, destinationPath));
        }

        public void DeleteFile(string filePath) =>
            File.Delete(filePath);
    }
}
