using Microsoft.EntityFrameworkCore;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.Models;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
namespace TickAPI.Categories.Respositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public CategoryRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }

    public IQueryable<Category> GetCategories()
    {
        return _tickApiDbContext.Categories;
    }

    public async Task<Result<Category>> GetCategoryByNameAsync(string categoryName)
    {
        var category = await _tickApiDbContext.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);

        if (category == null)
        {
            return Result<Category>.Failure(StatusCodes.Status404NotFound, $"category with name '{categoryName}' not found");
        }
        
        return Result<Category>.Success(category);
    }

    public Result<IQueryable<Category>> GetCategoriesByNames(IEnumerable<string> categoryNames)
    {
        var categories = _tickApiDbContext.Categories.Where(c => categoryNames.Contains(c.Name));

        if (categories.Count() < categoryNames.Count())
        {
            return Result<IQueryable<Category>>.Failure(StatusCodes.Status404NotFound, "one or more of category names is invalid");
        }
        
        return Result<IQueryable<Category>>.Success(categories);
    }

    public async Task AddNewCategoryAsync(Category category)
    {
        _tickApiDbContext.Categories.Add(category);
        await _tickApiDbContext.SaveChangesAsync();
    }
}