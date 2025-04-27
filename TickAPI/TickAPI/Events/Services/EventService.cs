using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Request;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Response;
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

    public async Task<Result<Event>> CreateNewEventAsync(string name, string  description,  DateTime startDate, DateTime endDate, 
        uint? minimumAge, CreateAddressDto createAddress, List<CreateEventCategoryDto> categories, List<CreateEventTicketTypeDto> ticketTypes,
        EventStatus eventStatus, string organizerEmail)
    {
        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(organizerEmail);
        if (!organizerResult.IsSuccess)
            return Result<Event>.PropagateError(organizerResult);
        

        if (endDate < startDate)
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "End date should be after start date");
        
        if (startDate < _dateTimeService.GetCurrentDateTime())
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "Start date is in the past");

        if (ticketTypes.Any(t => t.AvailableFrom > endDate))
        {
            return Result<Event>.Failure(StatusCodes.Status400BadRequest, "Tickets can't be available after the event is over");
        }
        
        var address = await _addressService.GetOrCreateAddressAsync(createAddress);

        var categoryNames = categories.Select(c => c.CategoryName).ToList();

        var categoriesByNameResult = _categoryService.GetCategoriesByNames(categoryNames);
        
        if (categoriesByNameResult.IsError)
        {
            return Result<Event>.PropagateError(categoriesByNameResult);
        }

        var ticketTypesConverted = ticketTypes.Select(t => new TicketType
            {
                Description = t.Description,
                AvailableFrom = t.AvailableFrom,
                Currency = t.Currency,
                MaxCount = t.MaxCount,
                Price = t.Price,
            })
            .ToList();

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

    public async Task<Result<PaginatedData<GetEventResponseDto>>> GetOrganizerEventsAsync(Organizer organizer, int page, int pageSize)
    {
        var organizerEvents = _eventRepository.GetEventsByOranizer(organizer);
        return await GetPaginatedEventsAsync(organizerEvents, page, pageSize);
    }

    public async Task<Result<PaginationDetails>> GetOrganizerEventsPaginationDetailsAsync(Organizer organizer, int pageSize)
    {
        var organizerEvents = _eventRepository.GetEventsByOranizer(organizer);
        return await _paginationService.GetPaginationDetailsAsync(organizerEvents, pageSize);
    }

    public async Task<Result<PaginatedData<GetEventResponseDto>>> GetEventsAsync(int page, int pageSize)
    {
        var events = _eventRepository.GetEvents();
        return await GetPaginatedEventsAsync(events, page, pageSize);
    }

    public async Task<Result<PaginationDetails>> GetEventsPaginationDetailsAsync(int pageSize)
    {
        var events = _eventRepository.GetEvents();
        return await _paginationService.GetPaginationDetailsAsync(events, pageSize);
    }

    public async Task<Result<GetEventDetailsResponseDto>> GetEventDetailsAsync(Guid eventId)
    {
        var eventResult = _eventRepository.GetEventById(eventId);

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
    
    private static GetEventResponseDto MapEventToGetEventResponseDto(Event ev)
    {
        var categories = ev.Categories.Count > 0 ? ev.Categories.Select((c) => new GetEventResponseCategoryDto(c.Name)).ToList() : new List<GetEventResponseCategoryDto>(); 
        var address = new GetEventResponseAddressDto(ev.Address.Country, ev.Address.City, ev.Address.PostalCode, ev.Address.Street, ev.Address.HouseNumber, ev.Address.FlatNumber);
        
        var minimumPrice = ev.TicketTypes.Min(t => t.Price);
        var maximumPrice = ev.TicketTypes.Max(t => t.Price);
        
        return new GetEventResponseDto(ev.Id, ev.Name, ev.Description, ev.StartDate, ev.EndDate, ev.MinimumAge, 
            minimumPrice, maximumPrice, categories, ev.EventStatus, address);
    }
}