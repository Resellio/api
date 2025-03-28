using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Organizers.Abstractions;
using TickAPI.Events.DTOs.Request;

namespace TickAPI.Events.Services;

public class EventService : IEventService
{
    private readonly  IOrganizerService _organizerService;
    private readonly IEventRepository _eventRepository;
    private readonly IAddressService _addressService;

    public EventService(IEventRepository eventRepository, IOrganizerService organizerService, IAddressService addressService)
    {
        _eventRepository = eventRepository;
        _organizerService = organizerService;
        _addressService = addressService;
    }

    public async Task<Result<Event>> CreateNewEventAsync(string name, string  description,  string startDate, string endDate, uint? minimumAge, CreateAddressDto createAddress, EventStatus eventStatus, string organizerEmail)
    {
        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(organizerEmail);
        if (!organizerResult.IsSuccess)
            return Result<Event>.PropagateError(organizerResult);

        if (!DateTime.TryParse(startDate, new System.Globalization.CultureInfo("fr-FR"), 
                System.Globalization.DateTimeStyles.None, out DateTime startDateParsed))
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "Invalid start date format");
        
        if (!DateTime.TryParse(endDate, new System.Globalization.CultureInfo("fr-FR"), 
                System.Globalization.DateTimeStyles.None, out DateTime endDateParsed))
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "Invalid end date format");

        if (endDateParsed < startDateParsed)
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "End date should be after start date");
        
        var address = await _addressService.GetAddressAsync(createAddress);
        
        var @event = new Event
        {
            Name = name,
            Description = description,
            StartDate = startDateParsed,
            EndDate = endDateParsed,
            MinimumAge = minimumAge,
            Address = address.Value!,
            Organizer = organizerResult.Value!,
            EventStatus = eventStatus
        };
        await _eventRepository.AddNewEventAsync(@event);
        return Result<Event>.Success(@event);
    }
}