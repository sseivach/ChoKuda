using ChoKuda.App.Services;

namespace ChoKuda.App.Tests.Services;

public sealed class AttachmentDropServiceTests
{
    [Fact]
    public void DropFilesRaisesExistingDistinctFiles()
    {
        var service = new AttachmentDropService();
        var filePath = Path.GetTempFileName();
        var duplicatePath = filePath.ToUpperInvariant();
        IReadOnlyList<string>? droppedFilePaths = null;
        service.FilesDropped += (_, eventArgs) => droppedFilePaths = eventArgs.FilePaths;

        try
        {
            service.DropFiles([filePath, duplicatePath, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))]);

            Assert.NotNull(droppedFilePaths);
            Assert.Equal([filePath], droppedFilePaths);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void DropFilesIgnoresDirectoriesAndMissingFiles()
    {
        var service = new AttachmentDropService();
        var directoryPath = Directory.CreateTempSubdirectory().FullName;
        var eventWasRaised = false;
        service.FilesDropped += (_, _) => eventWasRaised = true;

        try
        {
            service.DropFiles([directoryPath, Path.Combine(directoryPath, "missing.pdf")]);

            Assert.False(eventWasRaised);
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
