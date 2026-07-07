namespace ChoKuda.Core.Points;

public interface IPointFileSystem
{
    bool FileExists(string filePath);

    void DeleteFile(string filePath);
}
