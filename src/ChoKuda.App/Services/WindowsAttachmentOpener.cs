using System.Diagnostics;
using System.IO;

namespace ChoKuda.App.Services;

public sealed class WindowsAttachmentOpener : IAttachmentOpener
{
    public string? Open(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return "File was not found.";
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
            });

            return null;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return $"File could not be opened: {exception.Message}";
        }
    }
}
