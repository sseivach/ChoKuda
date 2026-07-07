using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class AttachmentFileNameTests
{
    private static readonly Guid AttachmentId =
        Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

    [Theory]
    [InlineData("waterfall.jpg", "waterfall__550e8400-e29b-41d4-a716-446655440000.jpg")]
    [InlineData("my photo.png", "my photo__550e8400-e29b-41d4-a716-446655440000.png")]
    [InlineData("guide", "guide__550e8400-e29b-41d4-a716-446655440000")]
    public void CreateStoredNameKeepsOriginalNameAndAddsGuidBeforeExtension(
        string originalName,
        string expected)
    {
        var storedName = AttachmentFileName.CreateStoredName(originalName, AttachmentId);

        Assert.Equal(expected, storedName);
    }

    [Fact]
    public void CreateStoredNameRemovesWindowsInvalidCharacters()
    {
        var storedName = AttachmentFileName.CreateStoredName(
            "wa\\te/r:fa*ll?\"<>|.jpg",
            AttachmentId);

        Assert.Equal("waterfall__550e8400-e29b-41d4-a716-446655440000.jpg", storedName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\\/:*?\"<>|")]
    public void CreateStoredNameUsesFileWhenNameIsMissingOrEmptyAfterSanitization(
        string originalName)
    {
        var storedName = AttachmentFileName.CreateStoredName(originalName, AttachmentId);

        Assert.Equal("file__550e8400-e29b-41d4-a716-446655440000", storedName);
    }

    [Fact]
    public void CreateStoredNameSanitizesInvalidCharactersInExtension()
    {
        var storedName = AttachmentFileName.CreateStoredName(
            "photo.jp?g",
            AttachmentId);

        Assert.Equal("photo__550e8400-e29b-41d4-a716-446655440000.jpg", storedName);
    }
}

