using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Search;

namespace ChoKuda.App.ViewModels;

public sealed class SearchViewModel
{
    public string Input { get; set; } = string.Empty;

    public string? ActiveQuery { get; private set; }

    public bool IsActive =>
        !string.IsNullOrWhiteSpace(ActiveQuery);

    public bool CanRunSearch =>
        !string.IsNullOrWhiteSpace(Input);

    public bool Run()
    {
        if (!CanRunSearch)
        {
            return false;
        }

        ActiveQuery = SearchService.NormalizeQuery(Input);
        return IsActive;
    }

    public void Reset()
    {
        Input = string.Empty;
        ActiveQuery = null;
    }

    public IReadOnlyList<PointDocument> Apply(IEnumerable<PointDocument> points) =>
        IsActive
            ? SearchService.Apply(points, ActiveQuery!)
            : points.ToArray();
}
