using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.ShoppingCarts.Abstractions;

namespace TickAPI.ShoppingCarts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingCartsController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartsController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpPost("ticket")]
    public async Task<ActionResult> AddTicket()
    {
        throw new NotImplementedException();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet("tickets")]
    public async Task<ActionResult> GetTickets()
    {
        throw new NotImplementedException();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpDelete("ticket")]
    public async Task<ActionResult> DeleteTicket()
    {
        throw new NotImplementedException();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpPost("checkout")]
    public async Task<ActionResult> Checkout()
    {
        throw new NotImplementedException();
    }
}