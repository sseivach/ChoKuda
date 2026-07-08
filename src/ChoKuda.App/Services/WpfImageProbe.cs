using ChoKuda.Core.Attachments;
using System.IO;
using System.Windows.Media.Imaging;

namespace ChoKuda.App.Services;

public sealed class WpfImageProbe : IImageProbe
{
    public bool CanDecodeImage(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            return decoder.Frames.Count > 0;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }
    }
}
