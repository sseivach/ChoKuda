using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Map;

public sealed record CollectionMapStyle(
    Guid CollectionId,
    string IconId,
    string Color,
    string IconColor = PointDefaults.DefaultPinIconColor);
