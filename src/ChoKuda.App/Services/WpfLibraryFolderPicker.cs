using System.Windows.Forms;

namespace ChoKuda.App.Services;

public sealed class WpfLibraryFolderPicker : ILibraryFolderPicker
{
    public string? PickFolder(string initialPath)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose ChoKuda library folder",
            SelectedPath = initialPath,
            UseDescriptionForTitle = true,
        };

        return dialog.ShowDialog() == DialogResult.OK
            ? dialog.SelectedPath
            : null;
    }
}

