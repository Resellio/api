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
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IClaimsService _claimsService;
    private readonly IOrganizerService _organizerService;

    public EventsController(IEventService eventService, IClaimsService claimsService, IOrganizerService organizerService)
    {
        _eventService = eventService;
        _claimsService = claimsService;
        _organizerService = organizerService;
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPost]
    public async Task<ActionResult<CreateEventResponseDto>> CreateEvent([FromBody] CreateEventDto request)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;
        
        var newEventResult = await _eventService.CreateNewEventAsync(request.Name, request.Description, 
            request.StartDate, request.EndDate, request.MinimumAge,  request.CreateAddress, request.Categories 
            , request.TicketTypes ,request.EventStatus, email);
        
        if (newEventResult.IsError)
            return StatusCode(newEventResult.StatusCode, newEventResult.ErrorMsg);
        
        return Ok("Event created succesfully");
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("organizer")]
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

        var paginatedDataResult = await _eventService.GetOrganizerEventsAsync(organizer, page, pageSize);
        if (paginatedDataResult.IsError)
        {
            return StatusCode(paginatedDataResult.StatusCode, paginatedDataResult.ErrorMsg);
        }

        return Ok(paginatedDataResult.Value!);
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("organizer-pagination-details")]
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

        var paginationDetailsResult = await _eventService.GetOrganizerEventsPaginationDetailsAsync(organizer, pageSize);
        if (paginationDetailsResult.IsError)
        {
            return StatusCode(paginationDetailsResult.StatusCode, paginationDetailsResult.ErrorMsg);
        }

        return Ok(paginationDetailsResult.Value!);
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet]
    public async Task<ActionResult<PaginatedData<GetEventResponseDto>>> GetEvents([FromQuery] int pageSize, [FromQuery] int page)
    {
        var paginatedDataResult = await _eventService.GetEventsAsync(page, pageSize);
        if (paginatedDataResult.IsError)
        {
            return StatusCode(paginatedDataResult.StatusCode, paginatedDataResult.ErrorMsg);
        }
        return Ok(paginatedDataResult.Value!);
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet("pagination-details")]
    public async Task<ActionResult<PaginationDetails>> GetEventsPaginationDetails([FromQuery] int pageSize)
    {
        var paginationDetailsResult = await _eventService.GetEventsPaginationDetailsAsync(pageSize);
        if (paginationDetailsResult.IsError)
        {
            return StatusCode(paginationDetailsResult.StatusCode, paginationDetailsResult.ErrorMsg);
        }
        return Ok(paginationDetailsResult.Value!);
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedUserPolicy)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetEventDetailsResponseDto>> GetEventDetails([FromRoute] Guid id)
    {
        var eventDetailsResult = await _eventService.GetEventDetailsAsync(id);
        if (eventDetailsResult.IsError)
        {
            return StatusCode(eventDetailsResult.StatusCode, eventDetailsResult.ErrorMsg);
        }
        return Ok(eventDetailsResult.Value!);
    }
}