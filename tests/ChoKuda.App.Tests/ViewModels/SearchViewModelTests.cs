using ChoKuda.App.ViewModels;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class SearchViewModelTests
{
    [Fact]
    public void EmptyInputCannotRunSearch()
    {
        var search = new SearchViewModel { Input = "   " };

        var ran = search.Run();

        Assert.False(ran);
        Assert.False(search.IsActive);
        Assert.False(search.CanRunSearch);
    }

    [Fact]
    public void RunNormalizesQueryAndApplyFiltersPoints()
    {
        var search = new SearchViewModel { Input = "  WATER  " };
        var points = new[]
        {
            new PointDocument { Title = "Waterfall", Latitude = 1, Longitude = 2 },
            new PointDocument { Title = "Desert", Latitude = 1, Longitude = 2 },
        };

        var ran = search.Run();
        var result = search.Apply(points);

        Assert.True(ran);
        Assert.True(search.IsActive);
        Assert.Equal(["Waterfall"], result.Select(point => point.Title));
    }

    [Fact]
    public void ResetClearsInputAndActiveQuery()
    {
        var search = new SearchViewModel { Input = "water" };
        search.Run();

        search.Reset();

        Assert.Equal(string.Empty, search.Input);
        Assert.False(search.IsActive);
    }
}
