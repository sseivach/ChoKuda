namespace ChoKuda.Core.FileLibrary;

public sealed class PointDocument
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string AddressRegion { get; set; } = string.Empty;

    public string DescriptionText { get; set; } = string.Empty;

    public List<Guid> CollectionIds { get; set; } = [];

    public Guid? PrimaryCollectionId { get; set; }

    public string TagsText { get; set; } = string.Empty;

    public List<string> Photos { get; set; } = [];

    public List<string> Files { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

