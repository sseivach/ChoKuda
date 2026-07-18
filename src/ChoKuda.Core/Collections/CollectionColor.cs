namespace ChoKuda.Core.Collections;

public static class CollectionColor
{
    public const string DefaultColor = "#2f75b5";

    public const string DefaultIconColor = "#ffffff";

    public static bool IsValid(string color) =>
        Normalize(color) is not null;

    public static string? Normalize(string color)
    {
        var trimmed = color.Trim();

        if (trimmed.Length != 7 || trimmed[0] != '#')
        {
            return null;
        }

        for (var index = 1; index < trimmed.Length; index++)
        {
            if (!Uri.IsHexDigit(trimmed[index]))
            {
                return null;
            }
        }

        return trimmed.ToLowerInvariant();
    }
}
