using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Tickets.DTOs.Request;

namespace TickAPI.Tickets.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly ITicketService  _ticketService;
    public TicketsController(IClaimsService claimsService, ITicketService ticketService)
    {
        _claimsService = claimsService;
        _ticketService = ticketService;
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetTicketDetailsResponseDto>> GetTicketDetails(Guid id)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;
        string? scanTicketUrl = Url.Action("ScanTicket", "Tickets", new { id = id }, Request.Scheme);
        var ticket = await _ticketService.GetTicketDetailsAsync(id, email, scanTicketUrl!);
        return ticket.ToObjectResult();
    }
    
    [HttpGet("for-resell")]
    public async Task<ActionResult<PaginatedData<GetTicketForResellResponseDto>>> GetTicketsForResell([FromQuery] Guid eventId, [FromQuery] int pageSize, [FromQuery] int page)
    {
        var result = await _ticketService.GetTicketsForResellAsync(eventId, page, pageSize);
        return result.ToObjectResult();
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet]
    public async Task<ActionResult<PaginatedData<GetTicketForCustomerDto>>> GetTicketsForCustomer([FromQuery] int pageSize, [FromQuery] int page, [FromQuery] TicketFiltersDto filters)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var tickets = await _ticketService.GetTicketsForCustomerAsync(emailResult.Value!, page, pageSize, filters);
        return tickets.ToObjectResult();
    }

    [HttpGet("scan/{id:guid}")]
    public async Task<ActionResult<bool>> ScanTicket(Guid id)
    {
       var res = await _ticketService.ScanTicket(id);
       return res.ToObjectResult();
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpPost("resell/{id:guid}")]
    public async Task<ActionResult<bool>> SetTicketForResell([FromRoute] Guid id, [FromBody] SetTicketForResellDataDto data)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var res = await _ticketService.SetTicketForResellAsync(id, emailResult.Value!, data.ResellPrice, data.ResellCurrency);
        return res.ToObjectResult();
    }
    
}