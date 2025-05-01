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
}