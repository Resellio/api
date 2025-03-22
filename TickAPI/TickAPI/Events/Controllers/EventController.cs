using Microsoft.AspNetCore.Mvc;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.DTOs.Request;
using System.Security.Claims;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Events.Models;
using TickAPI.Events.Abstractions;


namespace TickAPI.Events.Controllers;

[ApiController]
[Route("api/[controller]")]

// TODO: Add lists of categories and tickettypes
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }
    
    [AuthorizeWithPolicy(AuthPolicies.VerifiedOrganizerPolicy)]
    [HttpPost("create-event")]
    public async Task<ActionResult<CreateEventResponseDto>> CreateEvent([FromBody] CreateEventDto request)
    {
        DateTime startDate, endDate;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (email == null)
            return StatusCode(StatusCodes.Status400BadRequest, "missing email claim");
        Console.WriteLine(request.StartDate);
        if (!DateTime.TryParse(request.StartDate, out startDate))
            return BadRequest("Invalid start date format");
        
        if (!DateTime.TryParse(request.EndDate, out endDate))
            return BadRequest("Invalid end date format");
        
        var newEventResult = await _eventService.CreateNewEventAsync(request.Name, request.Description, startDate, endDate, request.MinimumAge, email, Address.FromDto(request.Address));
        
        if(newEventResult.IsError)
            return StatusCode(newEventResult.StatusCode, newEventResult.ErrorMsg);
        
        
        return new ActionResult<CreateEventResponseDto>(new CreateEventResponseDto());
    }
}