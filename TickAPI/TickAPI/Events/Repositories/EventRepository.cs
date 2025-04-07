using TickAPI.Common.TickApiDbContext;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;

namespace TickAPI.Events.Repositories;

public class EventRepository : IEventRepository
{
    
    private readonly TickApiDbContext _tickApiDbContext;

    public EventRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }
    public async Task AddNewEventAsync(Event @event)
    {
        _tickApiDbContext.Events.Add(@event);
        await _tickApiDbContext.SaveChangesAsync();
    }

    public IQueryable<Event> GetEvents()
    {
        return _tickApiDbContext.Events;
    }

    public IQueryable<Event> GetEventsByOranizer(Organizer organizer)
    {
        return _tickApiDbContext.Events.Where(e => e.Organizer.Id == organizer.Id);
    }
}