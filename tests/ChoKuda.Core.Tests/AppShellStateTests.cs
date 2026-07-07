using ChoKuda.Core;

namespace ChoKuda.Core.Tests;

public sealed class AppShellStateTests
{
    [Fact]
    public void InitialStateHasPanelsInDefaultLayout()
    {
        var state = new AppShellState();

        Assert.False(state.IsLeftPanelCollapsed);
        Assert.False(state.IsRightPanelOpen);
    }

    [Fact]
    public void ToggleLeftPanelCollapsesAndExpandsPanel()
    {
        var state = new AppShellState();

        state.ToggleLeftPanel();
        Assert.True(state.IsLeftPanelCollapsed);

        state.ToggleLeftPanel();
        Assert.False(state.IsLeftPanelCollapsed);
    }

    [Fact]
    public void RightPanelPreviewCanOpenAndClose()
    {
        var state = new AppShellState();

        state.OpenRightPanelPreview();
        Assert.True(state.IsRightPanelOpen);

        state.CloseRightPanel();
        Assert.False(state.IsRightPanelOpen);
    }

    [Fact]
    public void StateChangesRaiseChangedEvent()
    {
        var state = new AppShellState();
        var changeCount = 0;
        state.Changed += () => changeCount++;

        state.ToggleLeftPanel();
        state.OpenRightPanelPreview();
        state.CloseRightPanel();

        Assert.Equal(3, changeCount);
    }
}

