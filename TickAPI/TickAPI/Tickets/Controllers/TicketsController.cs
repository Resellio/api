using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;

namespace TickAPI.Tickets.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
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