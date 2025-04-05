using TickAPI.Categories.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Categories.Abstractions;

public interface ICategoryRepository
{
    public Task<ICollection<Category>> GetCategoriesAsync();
}