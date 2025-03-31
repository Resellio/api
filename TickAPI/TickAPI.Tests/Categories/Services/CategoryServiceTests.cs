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

    public async Task GetCategories_WhenDataIsValid_ShouldReturnOk()
    {
        //arrange
        int pageSize = 10;
        int page = 0;
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock.Setup(repo => repo.GetCategoriesAsync())
            .ReturnsAsync(Result<ICollection<Category>>.Success(new List<Category>()));

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.Paginate(new List<Category>(), pageSize, page)).Returns(
            Result<PaginatedData<Category>>.Success(new PaginatedData<Category>(new List<Category>(), page, pageSize,
                false, false, new PaginationDetails(0, 0)))
        );
        
        var sut = new CategoryService(categoryRepositoryMock.Object, paginationServiceMock.Object);
        
        //act
        var res = await sut.GetCategoriesResponsesAsync(pageSize, page);
        
        //assert
        var result = Assert.IsType<Result<PaginatedData<GetCategoryResponseDto>>>(res);
        Assert.True(result.IsSuccess);
    }
}