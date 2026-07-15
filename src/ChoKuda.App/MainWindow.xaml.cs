using System.IO;
using System.Windows;
using ChoKuda.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChoKuda.App;

public partial class MainWindow : Window
{
    private readonly AttachmentDropService _attachmentDropService;

    public MainWindow()
    {
        InitializeComponent();
        _attachmentDropService = ((IServiceProvider)FindResource("services"))
            .GetRequiredService<AttachmentDropService>();
    }

    private void HandlePreviewDragOver(object sender, System.Windows.DragEventArgs eventArgs)
    {
        eventArgs.Effects = GetDroppedFiles(eventArgs.Data).Count > 0
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
        eventArgs.Handled = true;
    }

    private void HandlePreviewDrop(object sender, System.Windows.DragEventArgs eventArgs)
    {
        var filePaths = GetDroppedFiles(eventArgs.Data);

        if (filePaths.Count > 0)
        {
            _attachmentDropService.DropFiles(filePaths);
        }

        eventArgs.Handled = true;
    }

    private static IReadOnlyList<string> GetDroppedFiles(System.Windows.IDataObject dataObject)
    {
        if (!dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return Array.Empty<string>();
        }

        return dataObject.GetData(System.Windows.DataFormats.FileDrop) is string[] droppedPaths
            ? droppedPaths.Where(File.Exists).ToArray()
            : Array.Empty<string>();
    }
}
