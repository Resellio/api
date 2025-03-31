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
        var res = await  _categoryRepository.GetCategoriesAsync();
        List<GetCategoryResponseDto> categories = new List<GetCategoryResponseDto>();
        var categoriesPaginated = _paginationService.Paginate<Category>(res.Value, pageSize, page);
        if (!categoriesPaginated.IsSuccess)
        {
            return Result<PaginatedData<GetCategoryResponseDto>>.PropagateError(categoriesPaginated);
        }
        
        foreach (var category in categoriesPaginated.Value.Data)
        {
            categories.Add(new GetCategoryResponseDto(category.CategoryName));
        }
        
        return Result<PaginatedData<GetCategoryResponseDto>>.Success(new PaginatedData<GetCategoryResponseDto>(categories, categoriesPaginated.Value.PageNumber
        ,categoriesPaginated.Value.PageSize, categoriesPaginated.Value.HasNextPage, categoriesPaginated.Value.HasPreviousPage,
        categoriesPaginated.Value.PaginationDetails));
    }
}