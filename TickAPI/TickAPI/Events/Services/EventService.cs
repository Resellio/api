using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Organizers.Abstractions;

namespace TickAPI.Events.Services;

public class EventService : IEventService
{
    private readonly IDateTimeService  _dateTimeService;
    private readonly  IOrganizerService _organizerService;
    private readonly IEventRepository _eventRepository;

    public EventService(IDateTimeService dateTimeService, IEventRepository eventRepository, IOrganizerService organizerService)
    {
        _dateTimeService = dateTimeService;
        _eventRepository = eventRepository;
        _organizerService = organizerService;
    }

    public async Task<Result<Event>> CreateNewEventAsync(string name, string  description,  DateTime startDate, DateTime endDate, uint? minimumAge, string organizerEmail, Address address)
    {
        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(organizerEmail);
        if (!organizerResult.IsSuccess)
            return Result<Event>.Failure(StatusCodes.Status400BadRequest,
                $"organizer with email '{organizerEmail}' doesn't exist");
        
        var @event = new Event
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            MinimumAge = minimumAge,
            Address = address,
            Organizer = organizerResult.Value!
        };
        await _eventRepository.AddNewEventAsync(@event);
        return Result<Event>.Success(@event);
    }
}