namespace ChoKuda.App.Services;

public interface IAttachmentFilePicker
{
    IReadOnlyList<string> PickFiles();
}
