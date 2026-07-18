using System.Text.Json.Serialization;

namespace ChoKuda.Core.FileLibrary;

public sealed class CollectionDocument
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon_id")]
    public string IconId { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("icon_color")]
    public string IconColor { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string DescriptionText { get; set; } = string.Empty;
}
