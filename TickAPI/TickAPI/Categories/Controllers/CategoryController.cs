using Microsoft.AspNetCore.Mvc;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.Models;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Customers.Abstractions;

namespace TickAPI.Categories.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoryController : Controller
{
    
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPost("get-categories")]
    public async Task<IEnumerable<Category>> GetCategories()
    {
        throw new NotImplementedException();
    }
}