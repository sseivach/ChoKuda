using ChoKuda.App.Services;
using ChoKuda.Core;
using ChoKuda.Core.Attachments;
using ChoKuda.Core.Collections;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Points;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace ChoKuda.App;

public partial class App : System.Windows.Application
{
    public App()
    {
        var services = new ServiceCollection();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var environmentPaths = new AppEnvironmentPaths(appDataPath, userProfilePath);

        services.AddWpfBlazorWebView();
        services.AddSingleton<AppShellState>();
        services.AddSingleton(environmentPaths);
        services.AddSingleton(new AppSettingsService(environmentPaths.AppSettingsFilePath));
        services.AddSingleton<FileLibraryService>();
        services.AddSingleton<LibraryCoordinator>();
        services.AddSingleton<PointService>();
        services.AddSingleton<CollectionService>();
        services.AddSingleton<IImageProbe, WpfImageProbe>();
        services.AddSingleton<AttachmentFileClassifier>();
        services.AddSingleton<AttachmentImportService>();
        services.AddSingleton<AttachmentDeleteService>();
        services.AddSingleton<AttachmentDropService>();
        services.AddSingleton<IAttachmentFilePicker, WpfAttachmentFilePicker>();
        services.AddSingleton<IAttachmentOpener, WindowsAttachmentOpener>();
        services.AddSingleton<IPathOpener, WindowsPathOpener>();
        services.AddSingleton<BootstrapIconCatalog>();
        services.AddSingleton<ILibraryFolderPicker, WpfLibraryFolderPicker>();
        services.AddSingleton<IApplicationShutdown, WpfApplicationShutdown>();

        Resources.Add("services", services.BuildServiceProvider());
    }
}
