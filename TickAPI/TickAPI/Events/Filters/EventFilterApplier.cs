using TickAPI.Events.Abstractions;
using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;

namespace TickAPI.Events.Filters;

public class EventFilterApplier : IEventFilterApplier
{
    private readonly IEventFilter _eventFilter;
    private readonly Dictionary<Func<EventFiltersDto, bool>, Action<EventFiltersDto>> _filterActions;

    public EventFilterApplier(IEventFilter eventFilter)
    {
        _eventFilter = eventFilter;
        _filterActions = new Dictionary<Func<EventFiltersDto, bool>, Action<EventFiltersDto>>
        {
            { f => !string.IsNullOrEmpty(f.Name), f => _eventFilter.FilterByName(f.Name!) },
            { f => !string.IsNullOrEmpty(f.Descritpion), f => _eventFilter.FilterByDescription(f.Descritpion!) },
            { f => f.StartDate.HasValue, f => _eventFilter.FilterByStartDate(f.StartDate!.Value) },
            { f => f.MinStartDate.HasValue, f => _eventFilter.FilterByMinStartDate(f.MinStartDate!.Value) },
            { f => f.MaxStartDate.HasValue, f => _eventFilter.FilterByMaxStartDate(f.MaxStartDate!.Value) },
            { f => f.EndDate.HasValue, f => _eventFilter.FilterByEndDate(f.EndDate!.Value) },
            { f => f.MinEndDate.HasValue, f => _eventFilter.FilterByMinEndDate(f.MinEndDate!.Value) },
            { f => f.MaxEndDate.HasValue, f => _eventFilter.FilterByMaxEndDate(f.MaxEndDate!.Value) },
            { f => f.MinPrice.HasValue, f => _eventFilter.FilterByMinPrice(f.MinPrice!.Value) },
            { f => f.MaxPrice.HasValue, f => _eventFilter.FilterByMaxPrice(f.MaxPrice!.Value) },
            { f => f.MinAge.HasValue, f => _eventFilter.FilterByMinAge(f.MinAge!.Value) },
            { f => f.MaxMinimumAge.HasValue, f => _eventFilter.FilterByMaxMinimumAge(f.MaxMinimumAge!.Value) },
            { f => !string.IsNullOrEmpty(f.AddressCountry), f => _eventFilter.FilterByAddressCountry(f.AddressCountry!) },
            { f => !string.IsNullOrEmpty(f.AddressCity), f => _eventFilter.FilterByAddressCity(f.AddressCity!) },
            { f => !string.IsNullOrEmpty(f.AddressStreet), f => _eventFilter.FilterByAddressStreet(
                f.AddressStreet!, 
                f.HouseNumber, 
                f.FlatNumber) },
            {f => !string.IsNullOrEmpty(f.PostalCode), f => _eventFilter.FilterByAddressPostalCode(f.PostalCode!)},
            {f => f.CategoriesNames is { Count: > 0 }, f => _eventFilter.FilterByCategoriesNames(f.CategoriesNames!)}
        };
    }

    public IQueryable<Event> ApplyFilters(EventFiltersDto filters)
    {   
        foreach (var (condition, apply) in _filterActions)
        {
            if (condition(filters))
            {
                apply(filters);
            }
        }

        return _eventFilter.GetEvents();
    }
}
