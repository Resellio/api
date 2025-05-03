using Microsoft.AspNetCore.Mvc;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs;
using TickAPI.Categories.DTOs.Response;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Pagination.Responses;

namespace TickAPI.Categories.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedUserPolicy)]
    [HttpGet]
    public async Task<ActionResult<PaginatedData<GetCategoryResponseDto>>> GetCategories([FromQuery] int pageSize, [FromQuery] int page)
    {
        var res = await _categoryService.GetCategoriesResponsesAsync(pageSize, page);
        if (res.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, res.ErrorMsg);
        }
        return Ok(res.Value);
    }
    
    [AuthorizeWithPolicy(AuthPolicies.AdminPolicy)]
    [HttpPost]
    public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryDto request)
    {
        var newCategoryResult = await _categoryService.CreateNewCategoryAsync(request.Name);
        
        if(newCategoryResult.IsError)
            return StatusCode(newCategoryResult.StatusCode, newCategoryResult.ErrorMsg);
        
        return Ok("category created successfully");
    }
}