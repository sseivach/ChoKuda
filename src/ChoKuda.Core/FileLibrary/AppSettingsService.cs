using System.Text.Json;

namespace ChoKuda.Core.FileLibrary;

public sealed class AppSettingsService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public AppSettingsService(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("App settings file path is required.", nameof(filePath));
        }

        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            return new AppSettings();
        }

        using var stream = File.OpenRead(_filePath);
        return JsonSerializer.Deserialize<AppSettings>(stream, _jsonOptions)
            ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var directoryPath = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var temporaryPath = $"{_filePath}.tmp";
        using (var stream = File.Create(temporaryPath))
        {
            JsonSerializer.Serialize(stream, settings, _jsonOptions);
        }

        File.Move(temporaryPath, _filePath, overwrite: true);
    }
}

