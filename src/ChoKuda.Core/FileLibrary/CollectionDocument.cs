namespace ChoKuda.Core.FileLibrary;

public sealed class CollectionDocument
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string IconId { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string DescriptionText { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

