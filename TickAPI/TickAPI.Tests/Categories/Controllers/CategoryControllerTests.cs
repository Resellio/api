using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.Controllers;
using TickAPI.Categories.DTOs;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
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
        // Arrange
        int pageSize = 20;
        int pageNumber = 0;
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock.Setup(m => m.GetCategoriesResponsesAsync(pageSize, pageNumber)).ReturnsAsync(
            Result<PaginatedData<GetCategoryResponseDto>>.Success(new PaginatedData<GetCategoryResponseDto>(new List<GetCategoryResponseDto>(), pageNumber, pageSize, true, true,
                new PaginationDetails(0, 0))));

        var sut = new CategoryController(categoryServiceMock.Object);
        
        // Act
        var res = await sut.GetCategories(pageSize, pageNumber);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetCategoryResponseDto>>>(res);
        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task CreateCategory_WhenDataIsValid_ShouldReturnSuccess()
    {
        // Arrange
        const string categoryName = "TestCategory";
        var createCategoryDto = new CreateCategoryDto(categoryName);
        
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(m => m.CreateNewCategoryAsync(categoryName))
            .ReturnsAsync(Result<Category>.Success(new Category()));
        
        var sut = new CategoryController(categoryServiceMock.Object);

        // Act
        var res = await sut.CreateCategory(createCategoryDto);

        // Assert
        var objectResult = Assert.IsType<OkObjectResult>(res);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal("category created successfully", objectResult.Value);
    }
}