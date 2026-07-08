using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;

namespace ChoKuda.Core.Tests.Filters;

public sealed class TagIndexTests
{
    [Fact]
    public void BuildReturnsDistinctSortedTagsFromPoints()
    {
        var points = new[]
        {
            new PointDocument { TagsText = "#safe #rentcar" },
            new PointDocument { TagsText = "#hard #safe" },
            new PointDocument { TagsText = "bad tag" },
            new PointDocument { TagsText = "" },
        };

        var tags = TagIndex.Build(points);

        Assert.Equal(["#hard", "#rentcar", "#safe"], tags);
    }
}
