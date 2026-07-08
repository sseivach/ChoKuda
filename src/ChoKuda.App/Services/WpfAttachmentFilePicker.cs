namespace ChoKuda.App.Services;

public sealed class WpfAttachmentFilePicker : IAttachmentFilePicker
{
    public IReadOnlyList<string> PickFiles()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = true,
            CheckFileExists = true,
            Title = "Add files",
            Filter = "All files (*.*)|*.*",
        };

        return dialog.ShowDialog() == true
            ? dialog.FileNames
            : Array.Empty<string>();
    }
}
