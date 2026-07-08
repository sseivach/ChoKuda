namespace ChoKuda.Core.Attachments;

public interface IAttachmentFileSystem
{
    bool FileExists(string filePath);

    void CopyFile(string sourcePath, string destinationPath);

    void DeleteFile(string filePath);
}
