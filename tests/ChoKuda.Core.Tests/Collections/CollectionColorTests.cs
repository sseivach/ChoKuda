using ChoKuda.Core.Collections;

namespace ChoKuda.Core.Tests.Collections;

public sealed class CollectionColorTests
{
    [Fact]
    public void DefaultsMatchCollectionStyleRules()
    {
        Assert.Equal("#2f75b5", CollectionColor.DefaultColor);
        Assert.Equal("#ffffff", CollectionColor.DefaultIconColor);
    }

    [Theory]
    [InlineData("#ABCDEF", "#abcdef")]
    [InlineData("#abcdef", "#abcdef")]
    [InlineData(" #123456 ", "#123456")]
    public void NormalizeAcceptsValidHexColors(string input, string expected)
    {
        Assert.True(CollectionColor.IsValid(input));
        Assert.Equal(expected, CollectionColor.Normalize(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("123456")]
    [InlineData("1234567")]
    [InlineData("#12345g")]
    public void NormalizeRejectsInvalidHexColors(string input)
    {
        Assert.False(CollectionColor.IsValid(input));
        Assert.Null(CollectionColor.Normalize(input));
    }
}
