using System.Diagnostics.CodeAnalysis;

namespace ChoKuda.Core.Attachments;

[ExcludeFromCodeCoverage]
public sealed class PhysicalAttachmentFileSystem : IAttachmentFileSystem
{
    public bool FileExists(string filePath) =>
        File.Exists(filePath);

    public void CopyFile(string sourcePath, string destinationPath) =>
        File.Copy(sourcePath, destinationPath, overwrite: false);

    public void DeleteFile(string filePath) =>
        File.Delete(filePath);
}
