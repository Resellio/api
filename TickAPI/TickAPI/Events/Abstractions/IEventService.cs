using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Categories.DTOs.Request;
using TickAPI.Common.Results;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Request;
using TickAPI.Events.DTOs.Response;
using TickAPI.Organizers.Models;
using TickAPI.TicketTypes.DTOs.Request;

namespace TickAPI.Events.Abstractions;

public interface IEventService
{
    public Task<Result<Event>> CreateNewEventAsync(string name, string description, DateTime startDate,
        DateTime endDate, uint? minimumAge, CreateAddressDto createAddress, List<CreateEventCategoryDto> categories,
        List<CreateEventTicketTypeDto> ticketTypes,EventStatus eventStatus, string organizerEmail, IFormFile? images);
    public Task<Result<PaginatedData<GetEventResponseDto>>> GetOrganizerEventsAsync(Organizer organizer, int page, int pageSize, EventFiltersDto? eventFilters = null);
    public Task<Result<PaginationDetails>> GetOrganizerEventsPaginationDetailsAsync(Organizer organizer, int pageSize);
    public Task<Result<PaginatedData<GetEventResponseDto>>> GetEventsAsync(int page, int pageSize, EventFiltersDto? eventFilters = null);
    public Task<Result<PaginationDetails>> GetEventsPaginationDetailsAsync(int pageSize);
    public Task<Result<GetEventDetailsResponseDto>> GetEventDetailsAsync(Guid eventId);
    public Task<Result<Event>> EditEventAsync(Organizer organizer, Guid eventId, string name, string description, DateTime startDate, DateTime endDate, uint? minimumAge, CreateAddressDto editAddress, List<EditEventCategoryDto> categories, EventStatus eventStatus);
    public Task<Result> SendMessageToParticipants(Organizer organizer, Guid eventId, string subject, string message);
    public Task<Result<GetEventDetailsOrganizerResponseDto>> GetEventDetailsOrganizerAsync(Guid eventId);
}