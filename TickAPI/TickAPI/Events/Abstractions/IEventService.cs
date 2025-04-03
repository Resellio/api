using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Response;
using TickAPI.Organizers.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventService
{
    public Task<Result<Event>> CreateNewEventAsync(string name, string description, DateTime startDate,
        DateTime endDate, uint? minimumAge, CreateAddressDto createAddress, EventStatus eventStatus, string organizerEmail);
    public Result<PaginatedData<GetEventResponseDto>> GetOrganizerEvents(Organizer organizer, int page, int pageSize);
    public Result<PaginationDetails> GetOrganizerEventsPaginationDetails(Organizer organizer, int pageSize);
}