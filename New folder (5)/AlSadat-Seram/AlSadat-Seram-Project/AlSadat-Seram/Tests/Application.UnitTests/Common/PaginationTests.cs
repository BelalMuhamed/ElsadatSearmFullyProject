using Application.CommonPagination;
using Xunit;

namespace Application.UnitTests.Common;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(10_000_000, 100)]
    public void PageSize_is_clamped_between_one_and_the_maximum(int requested, int expected)
    {
        Assert.Equal(expected, new PageRequest { PageSize = requested }.PageSize);
        Assert.Equal(expected, new PaginationParams { PageSize = requested }.PageSize);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(7, 7)]
    [InlineData(int.MaxValue, PageRequest.MaxPage)]
    public void Page_is_clamped(int requested, int expected)
    {
        Assert.Equal(expected, new PageRequest { Page = requested }.Page);
        Assert.Equal(expected, new PaginationParams { PageNumber = requested }.PageNumber);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(20, 1)]
    [InlineData(21, 2)]
    public void TotalPages_rounds_up(int totalCount, int expected)
    {
        Assert.Equal(expected, new PagedResult<int>([], totalCount, page: 1, pageSize: 20).TotalPages);
    }
}
