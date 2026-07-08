namespace ChoKuda.Core.Attachments;

public interface IImageProbe
{
    bool CanDecodeImage(string filePath);
}
