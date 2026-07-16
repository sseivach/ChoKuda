namespace ChoKuda.App.ViewModels;

public sealed record AttachmentStageResult(
    int ExistingFileCount,
    int AddedCount)
{
    public bool HasExistingFiles =>
        ExistingFileCount > 0;

    public bool HasAddedFiles =>
        AddedCount > 0;
}
