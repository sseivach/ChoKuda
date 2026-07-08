using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Search;

namespace ChoKuda.Core.Tests.Search;

public sealed class SearchServiceTests
{
    [Theory]
    [InlineData("  Havasu   Falls  ", "havasu falls")]
    [InlineData("\tRent\nCar ", "rent car")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void NormalizeQueryTrimsCompactsWhitespaceAndLowercases(
        string input,
        string expected)
    {
        Assert.Equal(expected, SearchService.NormalizeQuery(input));
    }

    [Fact]
    public void ApplyReturnsAllPointsForEmptyQuery()
    {
        var points = new[]
        {
            CreatePoint("B", "Arizona", "Text", "#safe"),
            CreatePoint("A", "Mexico", "Text", "#rentcar"),
        };

        var result = SearchService.Apply(points, "   ");

        Assert.Equal(["B", "A"], result.Select(point => point.Title));
    }

    [Fact]
    public void ApplyFindsPartialCaseInsensitiveTitleMatchesAndSortsByTitle()
    {
        var alphaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var betaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var points = new[]
        {
            CreatePoint("beta waterfall", id: betaId),
            CreatePoint("Alpha Waterfall", id: alphaId),
            CreatePoint("Desert overlook"),
        };

        var result = SearchService.Apply(points, "WATER");

        Assert.Equal(["Alpha Waterfall", "beta waterfall"], result.Select(point => point.Title));
    }

    [Fact]
    public void ApplySortsEqualTitlesById()
    {
        var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var points = new[]
        {
            CreatePoint("Same", id: secondId),
            CreatePoint("Same", id: firstId),
        };

        var result = SearchService.Apply(points, "same");

        Assert.Equal([firstId, secondId], result.Select(point => point.Id));
    }

    [Fact]
    public void MatchesAddressRegionAndDescription()
    {
        var point = CreatePoint(
            "Havasu Falls",
            address: "Arizona",
            description: "Permit required");

        Assert.True(SearchService.Matches(point, "rizo"));
        Assert.True(SearchService.Matches(point, "permit"));
        Assert.False(SearchService.Matches(point, "utah"));
    }

    [Fact]
    public void MatchesTagsWithOrWithoutHash()
    {
        var point = CreatePoint("Rental office", tags: "#rentcar #safe");

        Assert.True(SearchService.Matches(point, "rent"));
        Assert.True(SearchService.Matches(point, "#rent"));
        Assert.True(SearchService.Matches(point, "safe"));
        Assert.False(SearchService.Matches(point, "#hard"));
    }

    [Fact]
    public void MatchesReturnsTrueForEmptyQuery()
    {
        var point = CreatePoint("Havasu Falls");

        Assert.True(SearchService.Matches(point, " "));
    }

    [Fact]
    public void MatchesDoesNotUseFuzzyTransliterationOrMorphology()
    {
        var point = CreatePoint(
            "Waterfall",
            address: "Arizona",
            description: "Permit required",
            tags: "#rentcar");

        Assert.False(SearchService.Matches(point, "watrfall"));
        Assert.False(SearchService.Matches(point, "arizonaa"));
        Assert.False(SearchService.Matches(point, "vodopad"));
        Assert.False(SearchService.Matches(point, "rented"));
    }

    [Fact]
    public void TagsDoNotMatchWhenPointHasNoTags()
    {
        var point = CreatePoint("Havasu Falls", tags: "");

        Assert.False(SearchService.Matches(point, "#safe"));
    }

    [Fact]
    public void HashOnlyQueryDoesNotMatchTags()
    {
        var point = CreatePoint("Havasu Falls", tags: "#safe");

        Assert.False(SearchService.Matches(point, "#"));
    }

    private static PointDocument CreatePoint(
        string title,
        string address = "",
        string description = "",
        string tags = "",
        Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Latitude = 1,
            Longitude = 2,
            AddressRegion = address,
            DescriptionText = description,
            TagsText = tags,
        };
}
