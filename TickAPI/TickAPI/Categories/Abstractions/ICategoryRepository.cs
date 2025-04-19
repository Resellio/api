using TickAPI.Categories.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Categories.Abstractions;

public interface ICategoryRepository
{
    public IQueryable<Category> GetCategories();
    public Task<Result<Category>> GetCategoryByNameAsync(string categoryName);
    public Task AddNewCategoryAsync(Category category);
}