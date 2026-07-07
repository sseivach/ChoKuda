using System.Windows;
using ChoKuda.Core;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace ChoKuda.App;

public partial class App : Application
{
    public App()
    {
        var services = new ServiceCollection();
        services.AddWpfBlazorWebView();
        services.AddSingleton<AppShellState>();

        Resources.Add("services", services.BuildServiceProvider());
    }
}

