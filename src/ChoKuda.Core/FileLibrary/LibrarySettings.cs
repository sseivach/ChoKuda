namespace ChoKuda.Core.FileLibrary;

public sealed class LibrarySettings
{
    public int SchemaVersion { get; set; }

    public string MapProvider { get; set; } = string.Empty;

    public static LibrarySettings CreateDefault() =>
        new()
        {
            SchemaVersion = 1,
            MapProvider = "Stadia",
        };
}

