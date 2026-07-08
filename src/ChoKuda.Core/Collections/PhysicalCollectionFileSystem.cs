using System.Diagnostics.CodeAnalysis;

namespace ChoKuda.Core.Collections;

[ExcludeFromCodeCoverage]
public sealed class PhysicalCollectionFileSystem : ICollectionFileSystem
{
    public bool FileExists(string filePath) =>
        File.Exists(filePath);

    public void DeleteFile(string filePath) =>
        File.Delete(filePath);
}
