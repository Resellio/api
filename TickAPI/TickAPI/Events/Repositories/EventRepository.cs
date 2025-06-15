using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;

namespace TickAPI.Events.Repositories;

public class EventRepository : IEventRepository
{
    private readonly BaseEventRepository _baseEventRepository;
    private readonly TickApiDbContext _tickApiDbContext;

    public EventRepository(BaseEventRepository baseEventRepository, TickApiDbContext tickApiDbContext)
    {
        _baseEventRepository = baseEventRepository;
        _tickApiDbContext = tickApiDbContext;
    }

    public Task AddNewEventAsync(Event @event)
    {
        return _baseEventRepository.AddNewEventAsync(@event);
    }

    public async Task<IQueryable<Event>> GetEventsAsync()
    {
        var events = await _baseEventRepository.GetEventsAsync();
        await UpdateEventsStatuses(events);
        return events;
    }

    public async Task<IQueryable<Event>> GetEventsByOranizerAsync(Organizer organizer)
    {
        var events = await _baseEventRepository.GetEventsByOranizerAsync(organizer);
        await UpdateEventsStatuses(events);
        return events;
    }

    public async Task<Result<Event>> GetEventByIdAsync(Guid eventId)
    {
        var evResult = await _baseEventRepository.GetEventByIdAsync(eventId);
        if (evResult.IsError)
            return evResult;
        var ev = evResult.Value!;
        await UpdateEventStatuses([ev.Id]);
        return Result<Event>.Success(ev);
    }

    public Task<Result> SaveEventAsync(Event ev)
    {
        return _baseEventRepository.SaveEventAsync(ev);
    }

    public async Task<Result<Event>> GetEventByIdAndOrganizerAsync(Guid eventId, Organizer organizer)
    {
        var evResult = await _baseEventRepository.GetEventByIdAndOrganizerAsync(eventId, organizer);
        if (evResult.IsError)
            return evResult;
        var ev = evResult.Value!;
        await UpdateEventStatuses([ev.Id]);
        return Result<Event>.Success(ev);
    }

    public Task<decimal> GetEventRevenue(Guid eventId)
    {
        return _baseEventRepository.GetEventRevenue(eventId);
    }

    public Task<int> GetEventSoldTicketsCount(Guid eventId)
    {
        return _baseEventRepository.GetEventSoldTicketsCount(eventId);
    }

    private async Task UpdateEventStatuses(List<Guid> guids)
    {
        var eventsQuery = _tickApiDbContext.Events
            .Where(e => guids.Contains(e.Id));

        await UpdateEventsStatuses(eventsQuery);
    }

    private async Task UpdateEventsStatuses(IQueryable<Event> eventsQuery)
    {
        var now = DateTime.UtcNow;
        var events = await eventsQuery
            .Include(e => e.TicketTypes)
            .ThenInclude(tt => tt.Tickets)
            .ToListAsync();

        foreach (var ev in events)
        {
            if (ev.EndDate < now)
            {
                ev.EventStatus = EventStatus.Finished;
            }
            else if (ev.StartDate <= now && ev.EndDate >= now)
            {
                ev.EventStatus = EventStatus.InProgress;
            }
            else
            {
                var totalTickets = ev.TicketTypes.Sum(tt => tt.MaxCount);
                var soldTickets = ev.TicketTypes.Sum(tt => tt.Tickets.Count);
                if (totalTickets > 0 && soldTickets >= totalTickets)
                {
                    ev.EventStatus = EventStatus.SoldOut;
                }
                else
                {
                    ev.EventStatus = EventStatus.TicketsAvailable;
                }
            }
        }

        await _tickApiDbContext.SaveChangesAsync();
    }
}
