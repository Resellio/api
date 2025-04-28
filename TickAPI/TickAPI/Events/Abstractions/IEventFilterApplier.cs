using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventFilterApplier
{
    IQueryable<Event> ApplyFilters(IQueryable<Event> events, EventFiltersDto filters);
}
