namespace ChoKuda.App.ViewModels;

public enum UiOperationStatus
{
    Idle,
    Loading,
    Success,
    Error,
}

public enum UiRetryAction
{
    PrepareLibrary,
    ReloadLibrary,
}

public sealed class UiOperationViewModel
{
    public UiOperationStatus Status { get; private set; } = UiOperationStatus.Idle;

    public string Message { get; private set; } = "Ready.";

    public UiRetryAction? RetryAction { get; private set; }

    public bool CanRetry =>
        Status == UiOperationStatus.Error && RetryAction is not null;

    public void SetIdle(string message = "Ready.")
    {
        Status = UiOperationStatus.Idle;
        Message = message;
        RetryAction = null;
    }

    public void SetLoading(string message)
    {
        Status = UiOperationStatus.Loading;
        Message = message;
        RetryAction = null;
    }

    public void SetSuccess(string message)
    {
        Status = UiOperationStatus.Success;
        Message = message;
        RetryAction = null;
    }

    public void SetError(string message, UiRetryAction? retryAction = null)
    {
        Status = UiOperationStatus.Error;
        Message = message;
        RetryAction = retryAction;
    }
}
