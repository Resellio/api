using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
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
    private readonly IDateTimeService _dateTimeService;

    public EventService(IEventRepository eventRepository, IOrganizerService organizerService, IAddressService addressService, IDateTimeService dateTimeService)
    {
        _eventRepository = eventRepository;
        _organizerService = organizerService;
        _addressService = addressService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<Event>> CreateNewEventAsync(string name, string  description,  DateTime startDate, DateTime endDate, uint? minimumAge, CreateAddressDto createAddress, EventStatus eventStatus, string organizerEmail)
    {
        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(organizerEmail);
        if (!organizerResult.IsSuccess)
            return Result<Event>.PropagateError(organizerResult);
        

        if (endDate < startDate)
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "End date should be after start date");
        
        if (startDate < _dateTimeService.GetCurrentDateTime())
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "Start date is in the past");
        
        
        var address = await _addressService.GetOrCreateAddressAsync(createAddress);
        
        var @event = new Event
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            MinimumAge = minimumAge,
            Address = address.Value!,
            Organizer = organizerResult.Value!,
            EventStatus = eventStatus
        };
        await _eventRepository.AddNewEventAsync(@event);
        return Result<Event>.Success(@event);
    }
}