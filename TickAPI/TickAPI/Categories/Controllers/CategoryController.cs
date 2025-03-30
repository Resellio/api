using Microsoft.AspNetCore.Mvc;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Categories.Models;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Abstractions;

namespace TickAPI.Categories.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoryController : Controller
{
    
    private readonly ICategoryService _categoryService;
    private readonly IPaginationService _paginationService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPost("get-categories")]
    public async Task<ActionResult<IEnumerable<GetCategoriesDto>>> GetCategories()
    {
        var res = await _categoryService.GetCategoriesAsync();
        if (!res.IsSuccess)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, res.ErrorMsg);
        }

        return Ok(res.Value);
    }
}