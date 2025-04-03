using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Response;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;

namespace TickAPI.Events.Services;

public class EventService : IEventService
{
    private readonly  IOrganizerService _organizerService;
    private readonly IEventRepository _eventRepository;
    private readonly IAddressService _addressService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IPaginationService _paginationService;

    public EventService(IEventRepository eventRepository, IOrganizerService organizerService, IAddressService addressService, IDateTimeService dateTimeService, IPaginationService paginationService)
    {
        _eventRepository = eventRepository;
        _organizerService = organizerService;
        _addressService = addressService;
        _dateTimeService = dateTimeService;
        _paginationService = paginationService;
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

    public Result<PaginatedData<GetEventResponseDto>> GetOrganizerEvents(Organizer organizer, int page, int pageSize)
    {
        var paginatedEventsResult = _paginationService.Paginate(organizer.Events, pageSize, page);
        if (paginatedEventsResult.IsError)
        {
            return Result<PaginatedData<GetEventResponseDto>>.PropagateError(paginatedEventsResult);
        }

        var paginatedData = _paginationService.MapData(paginatedEventsResult.Value!, ev =>
        {
            var categories = ev.Categories.Select((c) => new GetEventResponseCategoryDto(c.CategoryName)).ToList();
            var address = new GetEventResponseAddressDto(ev.Address.Country, ev.Address.City, ev.Address.PostalCode, ev.Address.Street, ev.Address.HouseNumber, ev.Address.FlatNumber);
            return new GetEventResponseDto(ev.Name, ev.Description, ev.StartDate, ev.EndDate, ev.MinimumAge, categories, ev.EventStatus, address);
        });

        return Result<PaginatedData<GetEventResponseDto>>.Success(paginatedData);
    }

    public Result<PaginationDetails> GetOrganizerEventsPaginationDetails(Organizer organizer, int pageSize)
    {
        return _paginationService.GetPaginationDetails(organizer.Events!, pageSize);
    }
}