using Microsoft.AspNetCore.Mvc;
using TickAPI.Categories.Models;

namespace TickAPI.Categories.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoryController : Controller
{
    [HttpPost("get-categories")]
    public async Task<IEnumerable<Category>> GetCategories()
    {
        throw new NotImplementedException();
    }
}