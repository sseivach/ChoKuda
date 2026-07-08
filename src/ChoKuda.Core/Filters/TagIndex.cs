using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Filters;

public static class TagIndex
{
    public static IReadOnlyList<string> Build(IEnumerable<PointDocument> points)
    {
        return points
            .SelectMany(point => TagNormalization.Normalize(point.TagsText).NormalizedText.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}
