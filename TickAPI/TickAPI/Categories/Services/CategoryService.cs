using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IEnumerable<GetCategoriesDto>>> GetCategoriesAsync()
    {
        var res = await  _categoryRepository.GetCategoriesAsync();
        List<GetCategoriesDto> categories = new List<GetCategoriesDto>();
        foreach (var category in res.Value!)
        {
            categories.Add(new GetCategoriesDto(category.CategoryName));
        }
        return Result<IEnumerable<GetCategoriesDto>>.Success(categories);
    }
}