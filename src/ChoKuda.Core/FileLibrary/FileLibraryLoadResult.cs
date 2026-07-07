namespace ChoKuda.Core.FileLibrary;

public sealed class FileLibraryLoadResult<T>
{
    public FileLibraryLoadResult(
        IReadOnlyList<T> items,
        IReadOnlyList<FileLibraryLoadError> errors)
    {
        Items = items;
        Errors = errors;
    }

    public IReadOnlyList<T> Items { get; }

    public IReadOnlyList<FileLibraryLoadError> Errors { get; }

    public bool HasErrors => Errors.Count > 0;
}

