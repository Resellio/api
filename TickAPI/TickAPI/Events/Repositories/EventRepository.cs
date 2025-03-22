using Microsoft.EntityFrameworkCore;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;

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
}