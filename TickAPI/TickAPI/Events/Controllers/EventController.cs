using Microsoft.AspNetCore.Mvc;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.DTOs.Request;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Events.Abstractions;
using TickAPI.Organizers.Abstractions;

namespace TickAPI.Events.Controllers;

[ApiController]
[Route("api/[controller]")]

// TODO: Add lists of categories and tickettypes
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IClaimsService _claimsService;
    private readonly IOrganizerService _organizerService;

    public EventController(IEventService eventService, IClaimsService claimsService, IOrganizerService organizerService)
    {
        _eventService = eventService;
        _claimsService = claimsService;
        _organizerService = organizerService;
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
    public async Task<ActionResult<PaginatedData<GetEventResponseDto>>> GetOrganizerEvents([FromQuery] int pageSize, [FromQuery] int page)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;

        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
        {
            return StatusCode(organizerResult.StatusCode, organizerResult.ErrorMsg);
        }
        var organizer = organizerResult.Value!;

        var paginatedDataResult = _eventService.GetOrganizerEvents(organizer, page, pageSize);
        if (paginatedDataResult.IsError)
        {
            return StatusCode(paginatedDataResult.StatusCode, paginatedDataResult.ErrorMsg);
        }

        return Ok(paginatedDataResult.Value!);
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("get-organizer-events-pagination-details")]
    public async Task<ActionResult<PaginationDetails>> GetOrganizerEventsPaginationDetails([FromQuery] int pageSize)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;

        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
        {
            return StatusCode(organizerResult.StatusCode, organizerResult.ErrorMsg);
        }
        var organizer = organizerResult.Value!;

        var paginationDetailsResult = _eventService.GetOrganizerEventsPaginationDetails(organizer, pageSize);
        if (paginationDetailsResult.IsError)
        {
            return StatusCode(paginationDetailsResult.StatusCode, paginationDetailsResult.ErrorMsg);
        }

        return Ok(paginationDetailsResult.Value!);
    }
}