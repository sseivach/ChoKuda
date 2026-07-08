namespace ChoKuda.Core.Collections;

public interface ICollectionFileSystem
{
    bool FileExists(string filePath);

    void DeleteFile(string filePath);
}
