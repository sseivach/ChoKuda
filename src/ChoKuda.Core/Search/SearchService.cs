using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Search;

public static class SearchService
{
    public static string NormalizeQuery(string query) =>
        string.Join(
            ' ',
            query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToLowerInvariant();

    public static IReadOnlyList<PointDocument> Apply(
        IEnumerable<PointDocument> points,
        string query)
    {
        var normalizedQuery = NormalizeQuery(query);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return points.ToArray();
        }

        return points
            .Where(point => Matches(point, normalizedQuery))
            .OrderBy(point => point.Title, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(point => point.Id)
            .ToArray();
    }

    public static bool Matches(PointDocument point, string query)
    {
        var normalizedQuery = NormalizeQuery(query);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return true;
        }

        return Contains(point.Title, normalizedQuery) ||
            Contains(point.AddressRegion, normalizedQuery) ||
            Contains(point.DescriptionText, normalizedQuery) ||
            MatchesTags(point.TagsText, normalizedQuery);
    }

    private static bool Contains(string text, string normalizedQuery) =>
        text.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase);

    private static bool MatchesTags(string tagsText, string normalizedQuery)
    {
        var normalizedTags = TagNormalization.Normalize(tagsText).NormalizedText;

        if (string.IsNullOrWhiteSpace(normalizedTags))
        {
            return false;
        }

        var queryWithoutHash = normalizedQuery.Replace("#", string.Empty, StringComparison.Ordinal);
        var tagsWithoutHash = normalizedTags.Replace("#", string.Empty, StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(queryWithoutHash))
        {
            return false;
        }

        return normalizedTags.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
            tagsWithoutHash.Contains(queryWithoutHash, StringComparison.OrdinalIgnoreCase);
    }
}
