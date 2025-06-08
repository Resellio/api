using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventRepository
{
    public Task AddNewEventAsync(Event @event);
    public IQueryable<Event> GetEvents();
    public IQueryable<Event> GetEventsByOranizer(Organizer organizer);
    public Task<Result<Event>> GetEventByIdAsync(Guid eventId);
    public Task<Result> SaveEventAsync(Event ev);
    public Task<Result<Event>> GetEventByIdAndOrganizerAsync(Guid eventId, Organizer organizer);
    public Task<Result<decimal>> GetEventRevenue(Guid eventId);
    public Task<Result<int>> GetEventSoldTicketsCount(Guid eventId);
}