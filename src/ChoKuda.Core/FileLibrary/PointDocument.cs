using System.Text.Json.Serialization;

namespace ChoKuda.Core.FileLibrary;

public sealed class PointDocument
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("address")]
    public string AddressRegion { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string DescriptionText { get; set; } = string.Empty;

    [JsonPropertyName("collection_ids")]
    public List<Guid> CollectionIds { get; set; } = [];

    [JsonPropertyName("primary_collection_id")]
    public Guid? PrimaryCollectionId { get; set; }

    [JsonPropertyName("tags")]
    public string TagsText { get; set; } = string.Empty;

    [JsonPropertyName("photos")]
    public List<string> Photos { get; set; } = [];

    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = [];
}
