namespace ChoKuda.Core.Domain;

public sealed class TagNormalizationResult
{
    private TagNormalizationResult(string normalizedText, IReadOnlyList<string> invalidTokens)
    {
        NormalizedText = normalizedText;
        InvalidTokens = invalidTokens;
    }

    public string NormalizedText { get; }

    public IReadOnlyList<string> InvalidTokens { get; }

    public bool IsValid => InvalidTokens.Count == 0;

    public static TagNormalizationResult Valid(string normalizedText) =>
        new(normalizedText, Array.Empty<string>());

    public static TagNormalizationResult Invalid(IReadOnlyList<string> invalidTokens) =>
        new(string.Empty, invalidTokens);
}

