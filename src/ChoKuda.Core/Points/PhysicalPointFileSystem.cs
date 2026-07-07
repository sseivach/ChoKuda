using System.Diagnostics.CodeAnalysis;

namespace ChoKuda.Core.Points;

[ExcludeFromCodeCoverage]
public sealed class PhysicalPointFileSystem : IPointFileSystem
{
    public bool FileExists(string filePath) =>
        File.Exists(filePath);

    public void DeleteFile(string filePath) =>
        File.Delete(filePath);
}
