using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Request;

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
    public async Task<ActionResult> AddTicket([FromBody] AddNewTicketDto addNewTicketDto)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;

        var addTicketResult = await _shoppingCartService.AddNewTicketAsync(addNewTicketDto.TicketTypeId, email,
            addNewTicketDto.NameOnTicket, addNewTicketDto.Seats);
        if (addTicketResult.IsError)
        {
            return StatusCode(addTicketResult.StatusCode, addTicketResult.ErrorMsg);
        }

        return Ok();
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