using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPaginationService _paginationService;

    public CategoryService(ICategoryRepository categoryRepository,  IPaginationService paginationService)
    {
        _categoryRepository = categoryRepository;
        _paginationService = paginationService;
    }

    public Task<Result<Category>> GetCategoryByNameAsync(string categoryName)
    {
        return _categoryRepository.GetCategoryByNameAsync(categoryName);
    }

    public async Task<Result<PaginatedData<GetCategoryResponseDto>>> GetCategoriesResponsesAsync(int pageSize, int page)
    {
        var categoriesAllResponse = _categoryRepository.GetCategories();
        var categoriesPaginated = await _paginationService.PaginateAsync(categoriesAllResponse, pageSize, page);
        if (!categoriesPaginated.IsSuccess)
        {
            return Result<PaginatedData<GetCategoryResponseDto>>.PropagateError(categoriesPaginated);
        }
        
        var categoriesResponse = _paginationService.MapData(categoriesPaginated.Value!, (c) => new GetCategoryResponseDto(c.Name));
        
        return Result<PaginatedData<GetCategoryResponseDto>>.Success(categoriesResponse);
    }

    public async Task<Result<Category>> CreateNewCategoryAsync(string categoryName)
    {
        var alreadyExistingResult = await _categoryRepository.GetCategoryByNameAsync(categoryName);
        
        if (alreadyExistingResult.IsSuccess)
        {
            return Result<Category>.Failure(StatusCodes.Status400BadRequest, 
                $"category with name '{categoryName}' already exists");
        }

        var category = new Category()
        {
            Name = categoryName
        };
        
        await _categoryRepository.AddNewCategoryAsync(category);
        return Result<Category>.Success(category);
    }
}