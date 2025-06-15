using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Payment.Models;
using TickAPI.Customers.Abstractions;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Request;
using TickAPI.ShoppingCarts.DTOs.Response;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingCartsController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IClaimsService _claimsService;
    private readonly IMailService _mailService;
    private readonly ICustomerService _customerService;

    public ShoppingCartsController(IShoppingCartService shoppingCartService, IClaimsService claimsService, IMailService mailService, ICustomerService customerService)
    {
        _shoppingCartService = shoppingCartService;
        _claimsService = claimsService;
        _mailService = mailService;
        _customerService = customerService;
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
    [HttpPost("{ticketId:guid}")]
    public async Task<ActionResult> AddResellTicket([FromRoute] Guid ticketId)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var addTicketResult = await _shoppingCartService.AddResellTicketToCartAsync(ticketId, email);
        
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
    [HttpDelete("{ticketId:guid}")]
    public async Task<ActionResult> RemoveResellTicket([FromRoute] Guid ticketId)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var removeTicketResult = await _shoppingCartService.RemoveResellTicketFromCartAsync(ticketId, email);

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

        if (checkoutResult.IsError)
        {
            return checkoutResult.ToObjectResult();
        }

        var checkout = checkoutResult.Value!;

        var customerResult = await _customerService.GetCustomerByEmailAsync(email);
        if (customerResult.IsError)
        {
            return customerResult.ToObjectResult();
        }

        var customer = customerResult.Value!;
        
        await _mailService.SendTicketsAsync(customer, checkout.BoughtTickets);

        return checkout.PaymentResponse;
    }
}