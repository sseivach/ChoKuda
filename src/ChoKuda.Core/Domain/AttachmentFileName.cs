namespace ChoKuda.Core.Domain;

public static class AttachmentFileName
{
    private static readonly char[] InvalidFileNameChars =
        ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

    public static string CreateStoredName(string originalFileName, Guid attachmentId)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return $"file__{attachmentId:N}";
        }

        var sanitizedFileName = SanitizeFileName(originalFileName);
        var extension = Path.GetExtension(sanitizedFileName);
        var baseName = Path.GetFileNameWithoutExtension(sanitizedFileName);
        var sanitizedBaseName = SanitizeBaseName(baseName);
        var sanitizedExtension = SanitizeExtension(extension);

        if (string.IsNullOrWhiteSpace(sanitizedBaseName))
        {
            sanitizedBaseName = "file";
        }

        return $"{sanitizedBaseName}__{attachmentId:N}{sanitizedExtension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var sanitized = new string(fileName
            .Where(character => !InvalidFileNameChars.Contains(character))
            .ToArray());

        return sanitized.Trim();
    }

    private static string SanitizeBaseName(string baseName)
    {
        var sanitized = new string(baseName
            .Where(character => !InvalidFileNameChars.Contains(character))
            .ToArray());

        return sanitized.Trim();
    }

    private static string SanitizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        var sanitized = new string(extension
            .Where(character => !InvalidFileNameChars.Contains(character))
            .ToArray());

        return sanitized.Trim();
    }
}
