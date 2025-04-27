using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Categories.Abstractions;

public interface ICategoryService
{
    public Task<Result<Category>> GetCategoryByNameAsync(string categoryName);
    public Task<Result<PaginatedData<GetCategoryResponseDto>>> GetCategoriesResponsesAsync(int pageSize, int page);
    public Task<Result<Category>> CreateNewCategoryAsync(string categoryName);
    public Result<List<Category>> GetCategoriesByNames(IList<string> categoryNames);
}