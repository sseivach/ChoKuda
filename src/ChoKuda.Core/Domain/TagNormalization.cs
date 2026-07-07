namespace ChoKuda.Core.Domain;

public static class TagNormalization
{
    public static TagNormalizationResult Normalize(string tagsText)
    {
        if (string.IsNullOrWhiteSpace(tagsText))
        {
            return TagNormalizationResult.Valid(string.Empty);
        }

        var tokens = tagsText
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var invalidTokens = tokens
            .Where(token => !token.StartsWith('#') || token.Length == 1)
            .ToArray();

        if (invalidTokens.Length > 0)
        {
            return TagNormalizationResult.Invalid(invalidTokens);
        }

        var normalized = string.Join(
            ' ',
            tokens.Select(token => token.ToLowerInvariant()));

        return TagNormalizationResult.Valid(normalized);
    }
}

