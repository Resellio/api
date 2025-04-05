using Microsoft.EntityFrameworkCore;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.Models;
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

    public async Task<ICollection<Category>> GetCategoriesAsync()
    {
        var list = await _tickApiDbContext.Categories.ToListAsync();
        return list;
    }
}