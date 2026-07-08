using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.ViewModels;

public sealed record PointEditorSaveResult(
    PointDocument? Point,
    bool ShouldReloadLibrary)
{
    public bool IsSuccess =>
        Point is not null;

    public static PointEditorSaveResult Success(PointDocument point) =>
        new(point, ShouldReloadLibrary: false);

    public static PointEditorSaveResult Failure(bool shouldReloadLibrary = false) =>
        new(null, shouldReloadLibrary);
}
