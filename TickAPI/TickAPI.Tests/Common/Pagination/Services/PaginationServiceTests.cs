using Microsoft.AspNetCore.Http;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Pagination.Services;

namespace TickAPI.Tests.Common.Pagination.Services;

public class PaginationServiceTests
{
    private readonly PaginationService _paginationService = new();

    [Fact]
    public async Task Paginate_WhenPageSizeNegative_ShouldReturnFailure()
    {
        // Act
        var result = await _paginationService.PaginateAsync(new List<int>().AsQueryable(), -5, 0);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'pageSize' param must be > 0, got: -5", result.ErrorMsg);
    }
    
    [Fact]
    public async Task Paginate_WhenPageSizeZero_ShouldReturnFailure()
    {
        // Act
        var result = await _paginationService.PaginateAsync(new List<int>().AsQueryable(), 0, 0);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'pageSize' param must be > 0, got: 0", result.ErrorMsg);
    }
    
    [Fact]
    public async Task Paginate_WhenPageNegative_ShouldReturnFailure()
    {
        // Act
        var result = await _paginationService.PaginateAsync(new List<int>().AsQueryable(), 1, -12);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'page' param must be >= 0, got: -12", result.ErrorMsg);
    }

    [Fact]
    public async Task Paginate_WhenCollectionLengthSmallerThanPageSize_ShouldReturnAllElements()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 }.AsQueryable();
        int pageSize = data.Count() + 1;
        const int pageNumber = 0;
        
        // Act
        var result = await _paginationService.PaginateAsync(data, pageSize, pageNumber);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(data, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.False(result.Value?.HasNextPage);
        Assert.False(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count(), result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(0, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public async Task Paginate_WhenCollectionLengthBiggerThanPageSize_ShouldReturnPartOfCollection()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 }.AsQueryable();
        const int pageSize = 2;
        const int pageNumber = 0;
        
        // Act
        var result = await _paginationService.PaginateAsync(data, pageSize, pageNumber);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int> {1, 2}, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.True(result.Value?.HasNextPage);
        Assert.False(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count(), result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(2, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public async Task Paginate_WhenTakingElementsFromTheMiddle_ShouldReturnPaginatedDataWithBothBooleansTrue()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 }.AsQueryable();
        const int pageSize = 2;
        const int pageNumber = 1;
        
        // Act
        var result = await _paginationService.PaginateAsync(data, pageSize, pageNumber);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int> {3, 4}, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.True(result.Value?.HasNextPage);
        Assert.True(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count(), result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(2, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public async Task Paginate_WhenExceededMaxPageNumber_ShouldReturnFailure()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 }.AsQueryable();
        const int pageSize = 2;
        const int pageNumber = 3;
        
        // Act
        var result = await _paginationService.PaginateAsync(data, pageSize, pageNumber);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("'page' param must be <= 2, got: 3", result.ErrorMsg);
    }
    
    [Fact]
    public async Task Paginate_WhenOnLastPage_ShouldReturnHasNextPageSetToFalse()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 }.AsQueryable();
        const int pageSize = 2;
        const int pageNumber = 2;
        
        // Act
        var result = await _paginationService.PaginateAsync(data, pageSize, pageNumber);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int>() { 5 }, result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.False(result.Value?.HasNextPage);
        Assert.True(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count(), result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(2, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public async Task Paginate_WhenCollectionEmptyAndFirstPageIsRequested_ShouldReturnSuccess()
    {
        // Arrange
        var data = new List<int>().AsQueryable();
        const int pageSize = 2;
        const int pageNumber = 0;
        
        // Act
        var result = await _paginationService.PaginateAsync(data, pageSize, pageNumber);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new List<int>(), result.Value?.Data);
        Assert.Equal(pageNumber, result.Value?.PageNumber);
        Assert.Equal(pageSize, result.Value?.PageSize);
        Assert.False(result.Value?.HasNextPage);
        Assert.False(result.Value?.HasPreviousPage);
        Assert.Equal(data.Count(), result.Value?.PaginationDetails.AllElementsCount);
        Assert.Equal(0, result.Value?.PaginationDetails.MaxPageNumber);
    }

    [Fact]
    public void MapData_ShouldApplyLambdaToEachObject()
    {
        // Arrange
        var data = new List<int>() {1,2,3,4,5};
        var paginatedData = new PaginatedData<int>(data, 0, 5, true, false, new PaginationDetails(1, 10));
        Func<int, float> lambda = i => i * 2;
        var expectedData = new List<float>() { 2, 4, 6, 8, 10 };
        
        // Act
        var result = _paginationService.MapData(paginatedData, lambda);
        
        // Assert
        Assert.Equal(expectedData, result.Data);
        Assert.Equal(paginatedData.PageNumber, result.PageNumber);
        Assert.Equal(paginatedData.PageSize, result.PageSize);
        Assert.Equal(paginatedData.HasPreviousPage, result.HasPreviousPage);
        Assert.Equal(paginatedData.HasNextPage, result.HasNextPage);
        Assert.Equal(paginatedData.PaginationDetails, result.PaginationDetails);
    }
}