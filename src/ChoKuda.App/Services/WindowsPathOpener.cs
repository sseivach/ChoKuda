using System.Diagnostics;
using System.IO;

namespace ChoKuda.App.Services;

public sealed class WindowsPathOpener : IPathOpener
{
    public string? Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "Path is empty.";
        }

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return $"Path was not found: {path}";
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
            });

            return null;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return $"Path could not be opened: {exception.Message}";
        }
    }
}
