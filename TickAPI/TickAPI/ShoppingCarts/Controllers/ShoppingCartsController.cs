using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Payment.Models;
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
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var addTicketResult =
            await _shoppingCartService.AddNewTicketsToCartAsync(addTicketDto.TicketTypeId, addTicketDto.Amount,
                email);
        
        return addTicketResult.ToObjectResult();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet]
    public async Task<ActionResult<GetShoppingCartTicketsResponseDto>> GetTickets()
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var getTicketsResult = await _shoppingCartService.GetTicketsFromCartAsync(email);
        
        return getTicketsResult.ToObjectResult();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpDelete]
    public async Task<ActionResult> RemoveTickets([FromBody] RemoveNewTicketDto removeTicketDto)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var removeTicketResult =
            await _shoppingCartService.RemoveNewTicketsFromCartAsync(removeTicketDto.TicketTypeId, removeTicketDto.Amount,
                email);

        return removeTicketResult.ToObjectResult();
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet("due")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetDueAmount()
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var dueAmountResult = await _shoppingCartService.GetDueAmountAsync(email);

        return dueAmountResult.ToObjectResult();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpPost("checkout")]
    public async Task<ActionResult<PaymentResponsePG>> Checkout([FromBody] CheckoutDto checkoutDto)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var checkoutResult = await _shoppingCartService.CheckoutAsync(email, checkoutDto.Amount, checkoutDto.Currency,
            checkoutDto.CardNumber, checkoutDto.CardExpiry, checkoutDto.Cvv);

        return checkoutResult.ToObjectResult();
    }
}