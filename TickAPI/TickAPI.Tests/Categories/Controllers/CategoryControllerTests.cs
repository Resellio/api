using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.Controllers;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Response;

namespace TickAPI.Tests.Categories.Controllers;

public class CategoryControllerTests
{
    [Fact]

    public async Task GetCategories_WhenDataIsValid_ShouldReturnOk()
    {
        //arrange
        int pageSize = 20;
        int pageNumber = 0;
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock.Setup(m => m.GetCategoriesAsync(pageSize, pageNumber)).ReturnsAsync(
            Result<PaginatedData<GetCategoriesDto>>.Success(new PaginatedData<GetCategoriesDto>(new List<GetCategoriesDto>(), pageNumber, pageSize, true, true,
                new PaginationDetails(0, 0))));

        var sut = new CategoryController(categoryServiceMock.Object);
        
        //act
        var res = await sut.GetCategories(pageSize, pageNumber);
        
        //assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetCategoriesDto>>>(res);
        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }
    
}