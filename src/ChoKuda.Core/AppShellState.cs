namespace ChoKuda.Core;

public sealed class AppShellState
{
    public bool IsLeftPanelCollapsed { get; private set; }

    public bool IsRightPanelOpen { get; private set; }

    public event Action? Changed;

    public void ToggleLeftPanel()
    {
        IsLeftPanelCollapsed = !IsLeftPanelCollapsed;
        NotifyChanged();
    }

    public void OpenRightPanelPreview()
    {
        IsRightPanelOpen = true;
        NotifyChanged();
    }

    public void CloseRightPanel()
    {
        IsRightPanelOpen = false;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}

