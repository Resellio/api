using TickAPI.Events.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventFilter
{
    IQueryable<Event> FilterByName(IQueryable<Event> events, string name);
    IQueryable<Event> FilterByDescription(IQueryable<Event> events, string description);
    IQueryable<Event> FilterByStartDate(IQueryable<Event> events, DateTime startDate);
    IQueryable<Event> FilterByEndDate(IQueryable<Event> events, DateTime endDate);
    IQueryable<Event> FilterByMinPrice(IQueryable<Event> events, decimal minPrice);
    IQueryable<Event> FilterByMaxPrice(IQueryable<Event> events, decimal maxPrice);
    IQueryable<Event> FilterByMinAge(IQueryable<Event> events, uint minAge);
    IQueryable<Event> FilterByMaxAge(IQueryable<Event> events, uint maxAge);
    // TODO: add filters for address and categories
}
