using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Common.Pagination.Responses;

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
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;
        var ticket = await _ticketService.GetTicketDetailsAsync(id, email);
        if (ticket.IsError)
        {
            return  StatusCode(ticket.StatusCode, ticket.ErrorMsg);
        }
        return Ok(ticket.Value);
    }
    
    [HttpGet("/for-resell")]
    public async Task<ActionResult<PaginatedData<GetTicketForResellResponseDto>>> GetTicketsForResell([FromQuery] Guid eventId, [FromQuery] int pageSize, [FromQuery] int page)
    {
        var result = await _ticketService.GetTicketsForResellAsync(eventId, page, pageSize);
        if (result.IsError)
        {
            return StatusCode(result.StatusCode, result.ErrorMsg);
        }
        return result.Value!;
    }
}