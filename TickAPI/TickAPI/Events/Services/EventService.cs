using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Request;
using TickAPI.Common.Results;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Request;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.Filters;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;
using TickAPI.Tickets.Abstractions;
using TickAPI.TicketTypes.DTOs.Request;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Events.Services;

public class EventService : IEventService
{
    private readonly IOrganizerService _organizerService;
    private readonly IEventRepository _eventRepository;
    private readonly IAddressService _addressService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IPaginationService _paginationService;
    private readonly ICategoryService _categoryService;
    private readonly ITicketService _ticketService;

    public EventService(IEventRepository eventRepository, IOrganizerService organizerService, IAddressService addressService, IDateTimeService dateTimeService, IPaginationService paginationService, ICategoryService categoryService, ITicketService ticketService)
    {
        _eventRepository = eventRepository;
        _organizerService = organizerService;
        _addressService = addressService;
        _dateTimeService = dateTimeService;
        _paginationService = paginationService;
        _categoryService = categoryService;
        _ticketService = ticketService;
    }

    public async Task<Result<Event>> CreateNewEventAsync(string name, string  description, DateTime startDate, DateTime endDate, 
        uint? minimumAge, CreateAddressDto createAddress, List<CreateEventCategoryDto> categories, List<CreateEventTicketTypeDto> ticketTypes,
        EventStatus eventStatus, string organizerEmail)
    {
        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(organizerEmail);
        if (!organizerResult.IsSuccess)
            return Result<Event>.PropagateError(organizerResult);

        var ticketTypesConverted = ticketTypes.Select(t => new TicketType
            {
                Description = t.Description,
                AvailableFrom = t.AvailableFrom,
                Currency = t.Currency,
                MaxCount = t.MaxCount,
                Price = t.Price,
            })
            .ToList();
        
        var datesCheck = CheckEventDates(startDate, endDate, ticketTypesConverted);
        if (datesCheck.IsError)
            return Result<Event>.PropagateError(datesCheck);
        
        var address = await _addressService.GetOrCreateAddressAsync(createAddress);
        if (address.IsError)
        {
            return Result<Event>.PropagateError(address);
        }
        var categoryNames = categories.Select(c => c.CategoryName).ToList();
        var categoriesByNameResult = _categoryService.GetCategoriesByNames(categoryNames);
        if (categoriesByNameResult.IsError)
        {
            return Result<Event>.PropagateError(categoriesByNameResult);
        }
        
        var @event = new Event
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            MinimumAge = minimumAge,
            Address = address.Value!,
            Categories = categoriesByNameResult.Value!,
            Organizer = organizerResult.Value!,
            EventStatus = eventStatus,
            TicketTypes = ticketTypesConverted,
        };
        await _eventRepository.AddNewEventAsync(@event);
        return Result<Event>.Success(@event);
    }

    public async Task<Result<PaginatedData<GetEventResponseDto>>> GetOrganizerEventsAsync(Organizer organizer, int page, int pageSize, EventFiltersDto? eventFilters = null)
    {
        var organizerEvents = _eventRepository.GetEventsByOranizer(organizer);
        var filteredOrganizerEvents = ApplyEventFilters(organizerEvents, eventFilters);
        return await GetPaginatedEventsAsync(filteredOrganizerEvents, page, pageSize);
    }

    public async Task<Result<PaginationDetails>> GetOrganizerEventsPaginationDetailsAsync(Organizer organizer, int pageSize)
    {
        var organizerEvents = _eventRepository.GetEventsByOranizer(organizer);
        return await _paginationService.GetPaginationDetailsAsync(organizerEvents, pageSize);
    }

    public async Task<Result<PaginatedData<GetEventResponseDto>>> GetEventsAsync(int page, int pageSize, EventFiltersDto? eventFilters = null)
    {
        var events = _eventRepository.GetEvents();
        var filteredEvents = ApplyEventFilters(events, eventFilters);
        return await GetPaginatedEventsAsync(filteredEvents, page, pageSize);
    }

    public async Task<Result<PaginationDetails>> GetEventsPaginationDetailsAsync(int pageSize)
    {
        var events = _eventRepository.GetEvents();
        return await _paginationService.GetPaginationDetailsAsync(events, pageSize);
    }

    public async Task<Result<GetEventDetailsResponseDto>> GetEventDetailsAsync(Guid eventId)
    {
        var eventResult = await _eventRepository.GetEventByIdAsync(eventId);

        if (eventResult.IsError)
        {
            return Result<GetEventDetailsResponseDto>.PropagateError(eventResult);
        }

        var ev = eventResult.Value!;

        var categories = ev.Categories.Count > 0
            ? ev.Categories.Select((c) => new GetEventResponseCategoryDto(c.Name)).ToList()
            : new List<GetEventResponseCategoryDto>();
        
        var ticketTypes = ev.TicketTypes.Count > 0
            ? ev.TicketTypes.Select((t) => new GetEventDetailsResponseTicketTypeDto(t.Id, t.Description, t.Price,
                t.Currency, t.AvailableFrom, _ticketService.GetNumberOfAvailableTicketsByType(t).Value)).ToList()
            : new List<GetEventDetailsResponseTicketTypeDto>();
        
        var address = new GetEventResponseAddressDto(ev.Address.Country, ev.Address.City, ev.Address.PostalCode,
            ev.Address.Street, ev.Address.HouseNumber, ev.Address.FlatNumber);
        
        var details = new GetEventDetailsResponseDto(
            ev.Id,
            ev.Name,
            ev.Description,
            ev.StartDate,
            ev.EndDate,
            ev.MinimumAge,
            categories,
            ticketTypes,
            ev.EventStatus,
            address
        );
        
        return Result<GetEventDetailsResponseDto>.Success(details);
    }

    public async Task<Result<Event>> EditEventAsync(Organizer organizer, Guid eventId, string name, string description, DateTime startDate, DateTime endDate, uint? minimumAge, CreateAddressDto editAddress, List<EditEventCategoryDto> categories,
        EventStatus eventStatus)
    {
        var existingEventResult = await _eventRepository.GetEventByIdAndOrganizerAsync(eventId, organizer);
        if (existingEventResult.IsError)
        {
            return existingEventResult;
        }
        var existingEvent = existingEventResult.Value!;
        
        var datesCheck = CheckEventDates(startDate, endDate, existingEvent.TicketTypes, existingEvent.StartDate == startDate);
        if (datesCheck.IsError)
            return Result<Event>.PropagateError(datesCheck);

        var address = await _addressService.GetOrCreateAddressAsync(editAddress);
        if (address.IsError)
        {
            return Result<Event>.PropagateError(address);
        }
        
        var categoryNames = categories.Select(c => c.CategoryName).ToList();
        var categoriesByNameResult = _categoryService.GetCategoriesByNames(categoryNames);
        if (categoriesByNameResult.IsError)
        {
            return Result<Event>.PropagateError(categoriesByNameResult);
        }

        existingEvent.Name = name;
        existingEvent.Description = description;
        existingEvent.StartDate = startDate;
        existingEvent.EndDate = endDate;
        existingEvent.MinimumAge = minimumAge;
        existingEvent.Address = address.Value!;
        existingEvent.Categories = categoriesByNameResult.Value!;
        existingEvent.EventStatus = eventStatus;

        var saveResult = await _eventRepository.SaveEventAsync(existingEvent);
        if (saveResult.IsError)
        {
            return Result<Event>.PropagateError(saveResult);
        }

        return Result<Event>.Success(existingEvent);
    }

    private async Task<Result<PaginatedData<GetEventResponseDto>>> GetPaginatedEventsAsync(IQueryable<Event> events, int page, int pageSize)
    {
        var paginatedEventsResult = await _paginationService.PaginateAsync(events, pageSize, page);
        if (paginatedEventsResult.IsError)
        {
            return Result<PaginatedData<GetEventResponseDto>>.PropagateError(paginatedEventsResult);
        }

        var paginatedData = _paginationService.MapData(paginatedEventsResult.Value!, MapEventToGetEventResponseDto);

        return Result<PaginatedData<GetEventResponseDto>>.Success(paginatedData);
    }

    private IQueryable<Event> ApplyEventFilters(IQueryable<Event> events, EventFiltersDto? eventFilters = null)
    {
        if (eventFilters is null)
        {
            return events;
        }
        var ef = new EventFilter(events);
        var eventFiltersApplier = new EventFilterApplier(ef);
        var filteredEvents = eventFiltersApplier.ApplyFilters(eventFilters);
        return filteredEvents;
    }
    
    private static GetEventResponseDto MapEventToGetEventResponseDto(Event ev)
    {
        var categories = ev.Categories.Count > 0 ? ev.Categories.Select((c) => new GetEventResponseCategoryDto(c.Name)).ToList() : new List<GetEventResponseCategoryDto>(); 
        var address = new GetEventResponseAddressDto(ev.Address.Country, ev.Address.City, ev.Address.PostalCode, ev.Address.Street, ev.Address.HouseNumber, ev.Address.FlatNumber);
        
        // Here we assume that there is at least one ticket type in each event
        var ttMinimumPrice = ev.TicketTypes.MinBy(t => t.Price)!;
        var ttMaximumPrice = ev.TicketTypes.MaxBy(t => t.Price)!;

        var minimumPrice = new GetEventResponsePriceInfoDto(ttMinimumPrice.Price, ttMinimumPrice.Currency);
        var maximumPrice = new GetEventResponsePriceInfoDto(ttMaximumPrice.Price, ttMaximumPrice.Currency);
        
        return new GetEventResponseDto(ev.Id, ev.Name, ev.Description, ev.StartDate, ev.EndDate, ev.MinimumAge, 
            minimumPrice, maximumPrice, categories, ev.EventStatus, address);
    }

    private Result CheckEventDates(DateTime startDate, DateTime endDate, IEnumerable<TicketType> ticketTypes, bool skipStartDateEvaluation = false)
    {
        if (endDate < startDate)
            return Result.Failure(StatusCodes.Status400BadRequest, "End date should be after start date");
        
        if (!skipStartDateEvaluation && startDate < _dateTimeService.GetCurrentDateTime())
            return Result.Failure(StatusCodes.Status400BadRequest, "Start date is in the past");
        
        if (ticketTypes.Any(t => t.AvailableFrom > endDate))
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "Tickets can't be available after the event is over");
        }

        return Result.Success();
    }
}