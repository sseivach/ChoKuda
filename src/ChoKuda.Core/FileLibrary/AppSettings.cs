namespace ChoKuda.Core.FileLibrary;

public sealed class AppSettings
{
    public string? LibraryPath { get; set; }

    public string? StadiaApiKey { get; set; }

    public string? StadiaMapStyleId { get; set; }

    public bool UseLargeMapLabels { get; set; }
}
