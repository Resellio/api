using Microsoft.AspNetCore.Mvc;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.DTOs.Request;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Events.Abstractions;

namespace TickAPI.Events.Controllers;

[ApiController]
[Route("api/[controller]")]

// TODO: Add lists of categories and tickettypes
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IClaimsService _claimsService;

    public EventController(IEventService eventService, IClaimsService claimsService)
    {
        _eventService = eventService;
        _claimsService = claimsService;
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPost("create-event")]
    public async Task<ActionResult<CreateEventResponseDto>> CreateEvent([FromBody] CreateEventDto request)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;
        
        var newEventResult = await _eventService.CreateNewEventAsync(request.Name, request.Description, request.StartDate, request.EndDate, request.MinimumAge,  request.CreateAddress, request.EventStatus, email);
        
        if (newEventResult.IsError)
            return StatusCode(newEventResult.StatusCode, newEventResult.ErrorMsg);
        
        return Ok("Event created succesfully");
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("get-organizer-events")]
    public async Task<ActionResult<PaginatedData<string>>> GetOrganizerEvents()
    {
        throw new NotImplementedException();
    }
}