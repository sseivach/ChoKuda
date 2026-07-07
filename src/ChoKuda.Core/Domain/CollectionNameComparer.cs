namespace ChoKuda.Core.Domain;

public sealed class CollectionNameComparer : IComparer<CollectionSummary>
{
    public static CollectionNameComparer Instance { get; } = new();

    public int Compare(CollectionSummary? x, CollectionSummary? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var nameComparison = string.Compare(
            x.Name,
            y.Name,
            StringComparison.CurrentCultureIgnoreCase);

        return nameComparison != 0
            ? nameComparison
            : x.Id.CompareTo(y.Id);
    }
}

