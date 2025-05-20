using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Request;
using TickAPI.ShoppingCarts.DTOs.Response;

namespace TickAPI.ShoppingCarts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingCartsController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IClaimsService _claimsService;

    public ShoppingCartsController(IShoppingCartService shoppingCartService, IClaimsService claimsService)
    {
        _shoppingCartService = shoppingCartService;
        _claimsService = claimsService;
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpPost]
    public async Task<ActionResult> AddTickets([FromBody] AddNewTicketDto addTicketDto)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;

        var addTicketResult =
            await _shoppingCartService.AddNewTicketsToCartAsync(addTicketDto.TicketTypeId, addTicketDto.Amount,
                email);
        if (addTicketResult.IsError)
        {
            return StatusCode(addTicketResult.StatusCode, addTicketResult.ErrorMsg);
        }

        return Ok();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet]
    public async Task<ActionResult<GetShoppingCartTicketsResponseDto>> GetTickets()
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;

        var getTicketsResult = await _shoppingCartService.GetTicketsFromCartAsync(email);
        if (getTicketsResult.IsError)
        {
            return StatusCode(getTicketsResult.StatusCode, getTicketsResult.ErrorMsg);
        }

        return Ok(getTicketsResult.Value);
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpDelete]
    public async Task<ActionResult> RemoveTickets([FromBody] RemoveNewTicketDto removeTicketDto)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;

        var addTicketResult =
            await _shoppingCartService.RemoveNewTicketsFromCartAsync(removeTicketDto.TicketTypeId, removeTicketDto.Amount,
                email);
        if (addTicketResult.IsError)
        {
            return StatusCode(addTicketResult.StatusCode, addTicketResult.ErrorMsg);
        }

        return Ok();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpPost("checkout")]
    public async Task<ActionResult> Checkout()
    {
        throw new NotImplementedException();
    }
}