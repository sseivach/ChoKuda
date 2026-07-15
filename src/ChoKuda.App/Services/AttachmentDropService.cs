using System.IO;

namespace ChoKuda.App.Services;

public sealed class AttachmentDropService
{
    public event EventHandler<AttachmentFilesDroppedEventArgs>? FilesDropped;

    public void DropFiles(IEnumerable<string> filePaths)
    {
        var existingFilePaths = filePaths
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (existingFilePaths.Length == 0)
        {
            return;
        }

        FilesDropped?.Invoke(this, new AttachmentFilesDroppedEventArgs(existingFilePaths));
    }
}

public sealed class AttachmentFilesDroppedEventArgs : EventArgs
{
    public AttachmentFilesDroppedEventArgs(IReadOnlyList<string> filePaths)
    {
        FilePaths = filePaths;
    }

    public IReadOnlyList<string> FilePaths { get; }
}
