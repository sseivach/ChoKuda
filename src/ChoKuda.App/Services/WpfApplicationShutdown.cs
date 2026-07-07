namespace ChoKuda.App.Services;

public sealed class WpfApplicationShutdown : IApplicationShutdown
{
    public void Shutdown()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
