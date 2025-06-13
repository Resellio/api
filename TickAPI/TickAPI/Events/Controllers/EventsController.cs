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
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;
        
        var newEventResult = await _eventService.CreateNewEventAsync(request.Name, request.Description, 
            request.StartDate, request.EndDate, request.MinimumAge,  request.CreateAddress, request.Categories 
            , request.TicketTypes ,request.EventStatus, email, request.Image);

        if (newEventResult.IsError)
            return newEventResult.ToObjectResult();
        
        return Ok("Event created succesfully");
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("organizer")]
    public async Task<ActionResult<PaginatedData<GetEventResponseDto>>> GetOrganizerEvents([FromQuery] int pageSize, [FromQuery] int page, [FromQuery] EventFiltersDto eventFilters)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
        {
            return organizerResult.ToObjectResult();
        }
        var organizer = organizerResult.Value!;

        var paginatedDataResult = await _eventService.GetOrganizerEventsAsync(organizer, page, pageSize, eventFilters);
        return paginatedDataResult.ToObjectResult();
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("organizer-pagination-details")]
    public async Task<ActionResult<PaginationDetails>> GetOrganizerEventsPaginationDetails([FromQuery] int pageSize)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
        {
            return organizerResult.ToObjectResult();
        }
        var organizer = organizerResult.Value!;

        var paginationDetailsResult = await _eventService.GetOrganizerEventsPaginationDetailsAsync(organizer, pageSize);
        return paginationDetailsResult.ToObjectResult();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet]
    public async Task<ActionResult<PaginatedData<GetEventResponseDto>>> GetEvents([FromQuery] int pageSize, [FromQuery] int page, [FromQuery] EventFiltersDto eventFilters)
    {
        var paginatedDataResult = await _eventService.GetEventsAsync(page, pageSize, eventFilters);
        return paginatedDataResult.ToObjectResult();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet("pagination-details")]
    public async Task<ActionResult<PaginationDetails>> GetEventsPaginationDetails([FromQuery] int pageSize)
    {
        var paginationDetailsResult = await _eventService.GetEventsPaginationDetailsAsync(pageSize);
        return paginationDetailsResult.ToObjectResult();
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedUserPolicy)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetEventDetailsResponseDto>> GetEventDetails([FromRoute] Guid id)
    {
        var eventDetailsResult = await _eventService.GetEventDetailsAsync(id);
        return eventDetailsResult.ToObjectResult();
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpGet("organizer/{id:guid}")]
    public async Task<ActionResult<GetEventDetailsOrganizerResponseDto>> GetEventDetailsOrganizer([FromRoute] Guid id)
    {
        var eventDetailsResult = await _eventService.GetEventDetailsOrganizerAsync(id);
        return eventDetailsResult.ToObjectResult();
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<EditEventResponseDto>> EditEvent([FromRoute] Guid id, [FromBody] EditEventDto request)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
        {
            return organizerResult.ToObjectResult();
        }
        var organizer = organizerResult.Value!;

        var editedEventResult = await _eventService.EditEventAsync(organizer, id, request.Name, request.Description, request.StartDate, request.EndDate, request.MinimumAge,
            request.EditAddress, request.Categories, request.EventStatus);
        
        if (editedEventResult.IsError)
            return editedEventResult.ToObjectResult();
        
        return Ok("Event edited succesfully");
    }

    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPost("{id:guid}/message-to-participants")]
    public async Task<ActionResult> SendMessageToEventParticipants([FromRoute] Guid id, [FromBody] SendMessageToParticipantsDto request)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;

        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
        {
            return organizerResult.ToObjectResult();
        }
        var organizer = organizerResult.Value!;

        var result = await _eventService.SendMessageToParticipants(organizer, id, request.Subject, request.Message);
        return result.ToObjectResult();
    }
}