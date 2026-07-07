using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class TagNormalizationTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("#HardMode #SAFE #rentcar", "#hardmode #safe #rentcar")]
    [InlineData("  #hardmode   #safe\t#rentcar  ", "#hardmode #safe #rentcar")]
    [InlineData("#rent_car #rent-car", "#rent_car #rent-car")]
    public void NormalizeReturnsLowercaseTagsWithoutExtraSpaces(
        string input,
        string expected)
    {
        var result = TagNormalization.Normalize(input);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.NormalizedText);
        Assert.Empty(result.InvalidTokens);
    }

    [Fact]
    public void NormalizeRejectsWordsWithoutHash()
    {
        var result = TagNormalization.Normalize("#rent car hardmode");

        Assert.False(result.IsValid);
        Assert.Equal(string.Empty, result.NormalizedText);
        Assert.Equal(["car", "hardmode"], result.InvalidTokens);
    }

    [Fact]
    public void NormalizeRejectsHashWithoutTagName()
    {
        var result = TagNormalization.Normalize("# #safe");

        Assert.False(result.IsValid);
        Assert.Equal(["#"], result.InvalidTokens);
    }

    [Fact]
    public void TagNormalizationResultValidCreatesValidResult()
    {
        var result = TagNormalizationResult.Valid("#safe");

        Assert.True(result.IsValid);
        Assert.Equal("#safe", result.NormalizedText);
        Assert.Empty(result.InvalidTokens);
    }

    [Fact]
    public void TagNormalizationResultInvalidCreatesInvalidResult()
    {
        var result = TagNormalizationResult.Invalid(["safe"]);

        Assert.False(result.IsValid);
        Assert.Equal(string.Empty, result.NormalizedText);
        Assert.Equal(["safe"], result.InvalidTokens);
    }
}

