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

    public async Task<Result<PaginatedData<GetCategoryResponseDto>>> GetCategoriesResponsesAsync(int pageSize, int page)
    {
        var categoriesAllResponse = await _categoryRepository.GetCategoriesAsync();
        var categoriesPaginated = _paginationService.Paginate(categoriesAllResponse, pageSize, page);
        if (!categoriesPaginated.IsSuccess)
        {
            return Result<PaginatedData<GetCategoryResponseDto>>.PropagateError(categoriesPaginated);
        }
        
        var categoriesResponse = _paginationService.MapData(categoriesPaginated.Value!, (c) => new GetCategoryResponseDto(c.CategoryName));
        
        return Result<PaginatedData<GetCategoryResponseDto>>.Success(categoriesResponse);
    }
}