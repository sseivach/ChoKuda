using ChoKuda.App.ViewModels;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class UiOperationViewModelTests
{
    [Fact]
    public void StartsIdleWithoutRetry()
    {
        var operation = new UiOperationViewModel();

        Assert.Equal(UiOperationStatus.Idle, operation.Status);
        Assert.Equal("Ready.", operation.Message);
        Assert.False(operation.CanRetry);
        Assert.Null(operation.RetryAction);
    }

    [Fact]
    public void LoadingClearsRetry()
    {
        var operation = new UiOperationViewModel();
        operation.SetError("Failed.", UiRetryAction.ReloadLibrary);

        operation.SetLoading("Loading.");

        Assert.Equal(UiOperationStatus.Loading, operation.Status);
        Assert.Equal("Loading.", operation.Message);
        Assert.False(operation.CanRetry);
        Assert.Null(operation.RetryAction);
    }

    [Fact]
    public void ErrorCanExposeSafeRetryAction()
    {
        var operation = new UiOperationViewModel();

        operation.SetError("Library load failed.", UiRetryAction.ReloadLibrary);

        Assert.Equal(UiOperationStatus.Error, operation.Status);
        Assert.True(operation.CanRetry);
        Assert.Equal(UiRetryAction.ReloadLibrary, operation.RetryAction);
    }

    [Fact]
    public void SuccessClearsRetry()
    {
        var operation = new UiOperationViewModel();
        operation.SetError("Failed.", UiRetryAction.PrepareLibrary);

        operation.SetSuccess("Done.");

        Assert.Equal(UiOperationStatus.Success, operation.Status);
        Assert.Equal("Done.", operation.Message);
        Assert.False(operation.CanRetry);
        Assert.Null(operation.RetryAction);
    }
}
