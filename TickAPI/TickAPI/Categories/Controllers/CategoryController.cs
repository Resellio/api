using Microsoft.AspNetCore.Mvc;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Pagination.Responses;


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
    public async Task<ActionResult<PaginatedData<GetCategoriesDto>>> GetCategories([FromQuery] int pageSize, [FromQuery] int page)
    {
        var res = await _categoryService.GetCategoriesAsync(pageSize, page);
        if (!res.IsSuccess)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, res.ErrorMsg);
        }
        return Ok(res.Value);
    }
}