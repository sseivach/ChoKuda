using ChoKuda.Core.Attachments;

namespace ChoKuda.Core.Tests.Attachments;

public sealed class AttachmentFileClassifierTests
{
    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    [InlineData("photo.gif")]
    [InlineData("PHOTO.JPG")]
    public void ClassifyReturnsPhotoForDecodablePhotoCandidateExtensions(string fileName)
    {
        var classifier = new AttachmentFileClassifier(new FakeImageProbe(canDecodeImage: true));

        var kind = classifier.Classify(fileName);

        Assert.Equal(AttachmentKind.Photo, kind);
    }

    [Fact]
    public void ClassifyReturnsFileWhenPhotoCandidateCannotBeDecoded()
    {
        var classifier = new AttachmentFileClassifier(new FakeImageProbe(canDecodeImage: false));

        var kind = classifier.Classify("renamed-pdf.jpg");

        Assert.Equal(AttachmentKind.File, kind);
    }

    [Theory]
    [InlineData("guide.pdf")]
    [InlineData("archive.zip")]
    [InlineData("README")]
    public void ClassifyReturnsFileForNonPhotoExtensions(string fileName)
    {
        var probe = new RecordingImageProbe();
        var classifier = new AttachmentFileClassifier(probe);

        var kind = classifier.Classify(fileName);

        Assert.Equal(AttachmentKind.File, kind);
        Assert.Equal(0, probe.CallCount);
    }

    private sealed class FakeImageProbe : IImageProbe
    {
        private readonly bool _canDecodeImage;

        public FakeImageProbe(bool canDecodeImage)
        {
            _canDecodeImage = canDecodeImage;
        }

        public bool CanDecodeImage(string filePath) =>
            _canDecodeImage;
    }

    private sealed class RecordingImageProbe : IImageProbe
    {
        public int CallCount { get; private set; }

        public bool CanDecodeImage(string filePath)
        {
            CallCount++;
            return true;
        }
    }
}
