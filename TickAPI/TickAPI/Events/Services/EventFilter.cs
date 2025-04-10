using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;

namespace TickAPI.Events.Services;

public class EventFilter : IEventFilter
{

    public IQueryable<Event> FilterByName(IQueryable<Event> events, string name)
    {
        return events.Where(e => e.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public IQueryable<Event> FilterByDescription(IQueryable<Event> events, string description)
    
    {
        return events.Where(e => e.Description.Contains(description, StringComparison.CurrentCultureIgnoreCase));
    }

    public IQueryable<Event> FilterByStartDate(IQueryable<Event> events, DateTime startDate)
    {
        return events.Where(e => e.StartDate.Date == startDate.Date);
    }

    public IQueryable<Event> FilterByEndDate(IQueryable<Event> events, DateTime endDate)
    {
        return events.Where(e => e.EndDate.Date == endDate.Date);
    }

    public IQueryable<Event> FilterByMinPrice(IQueryable<Event> events, decimal minPrice)
    {
        return events.Where(e => e.TicketTypes.All(t => t.Price >= minPrice));
    }

    public IQueryable<Event> FilterByMaxPrice(IQueryable<Event> events, decimal maxPrice)
    {
        return events.Where(e => e.TicketTypes.All(t => t.Price <= maxPrice));
    }

    public IQueryable<Event> FilterByMinAge(IQueryable<Event> events, uint minAge)
    {
        return events.Where(e => e.MinimumAge >= minAge);
    }

    public IQueryable<Event> FilterByMaxAge(IQueryable<Event> events, uint maxAge)
    {
        return events.Where(e => e.MinimumAge <= maxAge);
    }
}
