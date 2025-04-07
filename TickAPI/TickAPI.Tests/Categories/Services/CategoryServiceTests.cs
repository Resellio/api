using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
using TickAPI.Categories.Services;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Tests.Categories.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetCategoriesResponsesAsync_WhenDataIsValid_ShouldReturnOk()
    {
        // Arrange
        int pageSize = 10;
        int page = 0;
        var allCategories = new List<Category>().AsQueryable(); 
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock.Setup(repo => repo.GetCategories())
            .Returns(allCategories);

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(allCategories, pageSize, page))
            .Returns(Task.FromResult(
                Result<PaginatedData<Category>>.Success(new PaginatedData<Category>(
                    new List<Category>(), 
                    page, 
                    pageSize,
                    false, 
                    false, 
                    new PaginationDetails(0, 0))
                )
            ));
        
        var sut = new CategoryService(categoryRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var res = await sut.GetCategoriesResponsesAsync(pageSize, page);
        
        // Assert
        var result = Assert.IsType<Result<PaginatedData<GetCategoryResponseDto>>>(res);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetCategoryByNameAsync_WhenCategoryWithNameIsReturnedFromRepository_ShouldReturnSuccess()
    {
        // Arrange
        const string categoryName = "TestCategory";

        var category = new Category()
        {
            Name = categoryName
        };
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock
            .Setup(m => m.GetCategoryByNameAsync(categoryName))
            .ReturnsAsync(Result<Category>.Success(category));
        
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new CategoryService(categoryRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var res = await sut.GetCategoryByNameAsync(categoryName);
        
        // Assert
        Assert.True(res.IsSuccess);
        Assert.Equal(category, res.Value);
    }
    
    [Fact]
    public async Task GetCategoryByNameAsync_WhenCategoryWithNameIsNotReturnedFromRepository_ShouldReturnFailure()
    {
        // Arrange
        const string categoryName = "TestCategory";
        const string errorMsg = $"category with name '{categoryName}' not found";
        const int statusCode = 404;
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock
            .Setup(m => m.GetCategoryByNameAsync(categoryName))
            .ReturnsAsync(Result<Category>.Failure(statusCode, errorMsg));
        
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new CategoryService(categoryRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var res = await sut.GetCategoryByNameAsync(categoryName);
        
        // Assert
        Assert.True(res.IsError);
        Assert.Equal(errorMsg, res.ErrorMsg);
        Assert.Equal(statusCode, res.StatusCode);
    }

    [Fact]
    public async Task CreateNewCategoryAsync_WhenCategoryDataIsValid_ShouldReturnNewCategory()
    {
        // Arrange
        const string categoryName = "TestCategory";
        const string errorMsg = $"category with name '{categoryName}' not found";
        const int statusCode = 404;
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock
            .Setup(m => m.GetCategoryByNameAsync(categoryName))
            .ReturnsAsync(Result<Category>.Failure(statusCode, errorMsg));
        
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new CategoryService(categoryRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var res = await sut.CreateNewCategoryAsync(categoryName);
        
        // Assert
        Assert.True(res.IsSuccess);
        Assert.Equal(categoryName, res.Value!.Name);
    }
    
    [Fact]
    public async Task CreateNewCategoryAsync_WhenWithNotUniqueName_ShouldReturnFailure()
    {
        // Arrange
        const string categoryName = "TestCategory";
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock
            .Setup(m => m.GetCategoryByNameAsync(categoryName))
            .ReturnsAsync(Result<Category>.Success(new Category()));
        
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new CategoryService(categoryRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var res = await sut.CreateNewCategoryAsync(categoryName);
        
        // Assert
        Assert.True(res.IsError);
        Assert.Equal(400, res.StatusCode);
        Assert.Equal($"category with name '{categoryName}' already exists", res.ErrorMsg);
    }
}