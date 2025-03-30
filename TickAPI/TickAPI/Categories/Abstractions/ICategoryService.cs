using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Categories.Abstractions;

public interface ICategoryService
{
    public Task<Result<PaginatedData<GetCategoriesDto>>> GetCategoriesAsync(int pageSize, int page);
}