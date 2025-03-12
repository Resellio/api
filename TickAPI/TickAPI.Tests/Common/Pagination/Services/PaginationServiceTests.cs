using Microsoft.AspNetCore.Http;
using TickAPI.Common.Pagination.Services;

namespace TickAPI.Tests.Common.Pagination.Services;

public class PaginationServiceTests
{
    private readonly PaginationService _paginationService = new();

    [Fact]
    public void Paginate_WhenPageSizeNegative_ShouldReturnFailure()
    {
        var result = _paginationService.Paginate(new List<int>(), -5, 0);
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'pageSize' param must be > 0, got: -5", result.ErrorMsg);
    }
    
    [Fact]
    public void Paginate_WhenPageSizeZero_ShouldReturnFailure()
    {
        var result = _paginationService.Paginate(new List<int>(), 0, 0);
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'pageSize' param must be > 0, got: 0", result.ErrorMsg);
    }
    
    [Fact]
    public void Paginate_WhenPageNegative_ShouldReturnFailure()
    {
        var result = _paginationService.Paginate(new List<int>(), 1, -12);
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'page' param must be >= 0, got: -12", result.ErrorMsg);
    }

    [Fact]
    public void Paginate_WhenCollectionLengthSmallerThanPageSize_ShouldReturnAllElements()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 };
        int pageSize = data.Count + 1;
        const int pageNumber = 0;
        
        var result = _paginationService.Paginate(data, pageSize, pageNumber);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(data, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.False(result.Value?.HasNextPage);
        Assert.False(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count, result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(0, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public void Paginate_WhenCollectionLengthBiggerThanPageSize_ShouldReturnPartOfCollection()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 };
        const int pageSize = 2;
        const int pageNumber = 0;
        
        var result = _paginationService.Paginate(data, pageSize, pageNumber);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int> {1, 2}, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.True(result.Value?.HasNextPage);
        Assert.False(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count, result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(2, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public void Paginate_WhenTakingElementsFromTheMiddle_ShouldReturnPaginatedDataWithBothBooleansTrue()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 };
        const int pageSize = 2;
        const int pageNumber = 1;
        
        var result = _paginationService.Paginate(data, pageSize, pageNumber);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int> {3, 4}, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.True(result.Value?.HasNextPage);
        Assert.True(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count, result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(2, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public void Paginate_WhenExceededMaxPageNumber_ShouldReturnFailure()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 };
        const int pageSize = 2;
        const int pageNumber = 3;
        
        var result = _paginationService.Paginate(data, pageSize, pageNumber);
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'page' param must be <= 2, got: 3", result.ErrorMsg);
    }
    
    [Fact]
    public void Paginate_WhenOnLastPage_ShouldReturnHasNextPageSetToFalse()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 };
        const int pageSize = 2;
        const int pageNumber = 2;
        
        var result = _paginationService.Paginate(data, pageSize, pageNumber);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int>() { 5 }, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.False(result.Value?.HasNextPage);
        Assert.True(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count, result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(2, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public void Paginate_WhenCollectionEmptyAndFirstPageIsRequested_ShouldReturnSuccess()
    {
        var data = new List<int>();
        const int pageSize = 2;
        const int pageNumber = 0;
        
        var result = _paginationService.Paginate(data, pageSize, pageNumber);

        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int>(), result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.False(result.Value?.HasNextPage);
        Assert.False(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count, result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(0, result.Value?.PaginationDetails.MaxPageNumber);
    }
}