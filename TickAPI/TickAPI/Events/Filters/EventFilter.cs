﻿using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;

namespace TickAPI.Events.Filters;

public class EventFilter : IEventFilter
{
    private IQueryable<Event> _events;

    public EventFilter(IQueryable<Event> events)
    {
        _events = events;
    }

    public IQueryable<Event> GetEvents() 
    {
        return _events;
    }

    public void FilterByNameOrDescription(string searchQuery)
    {
        _events = _events.Where(e => e.Name.ToLower().Contains(searchQuery.ToLower()) || e.Description.ToLower().Contains(searchQuery.ToLower()));
    }
    
    public void FilterByStartDate(DateTime startDate)
    {
        _events = _events.Where(e => e.StartDate.Date == startDate.Date);
    }

    public void FilterByMinStartDate(DateTime startDate)
    {
        _events = _events.Where(e => e.StartDate.Date >= startDate.Date);
    }

    public void FilterByMaxStartDate(DateTime startDate)
    {
        _events = _events.Where(e => e.StartDate.Date <= startDate.Date);
    }

    public void FilterByEndDate(DateTime endDate)
    {
        _events = _events.Where(e => e.EndDate.Date == endDate.Date);
    }

    public void FilterByMinEndDate(DateTime endDate)
    {
        _events = _events.Where(e => e.EndDate.Date >= endDate.Date);
    }

    public void FilterByMaxEndDate(DateTime endDate)
    {
        _events = _events.Where(e => e.EndDate.Date <= endDate.Date);
    }

    public void FilterByMinPrice(decimal minPrice)
    {
        _events = _events.Where(e => e.TicketTypes.Any(t => t.Price >= minPrice));
    }

    public void FilterByMaxPrice(decimal maxPrice)
    {
        _events = _events.Where(e => e.TicketTypes.Any(t => t.Price <= maxPrice));
    }

    public void FilterByMinAge(uint minAge)
    {
        _events = _events.Where(e => e.MinimumAge >= minAge);
    }

    public void FilterByMaxMinimumAge(uint maxMinimumAge)
    {
        _events = _events.Where(e => e.MinimumAge <= maxMinimumAge);
    }

    public void FilterByAddressCountry(string country)
    {
        _events = _events.Where(e => e.Address.Country.ToLower().Contains(country.ToLower()));
    }

    public void FilterByAddressCity(string city)
    {
        _events = _events.Where(e => e.Address.City.ToLower().Contains(city.ToLower()));
    }

    public void FilterByAddressStreet(string street, uint? houseNumber = null, uint? flatNumber = null)
    {
        var result = _events.Where(e => e.Address.Street != null && e.Address.Street.ToLower().Contains(street.ToLower()));
        if (houseNumber != null)
        {
            result = result.Where(e => e.Address.HouseNumber != null && e.Address.HouseNumber == houseNumber);
        }
        if (flatNumber != null)
        {
            result = result.Where(e => e.Address.FlatNumber != null && e.Address.FlatNumber == flatNumber);
        }
        _events = result;
    }

    public void FilterByAddressPostalCode(string postalCode)
    {
        _events = _events.Where(e => e.Address.PostalCode == postalCode);
    }

    public void FilterByCategoriesNames(IEnumerable<string> categoriesNames)
    {
        _events = _events.Where(e => e.Categories.Any(c => categoriesNames.Contains(c.Name)));
    }
}
