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

    public IQueryable<Event> FilterByAddressCountry(IQueryable<Event> events, string country)
    {
        return events.Where(e => e.Address.Country.Contains(country, StringComparison.CurrentCultureIgnoreCase));
    }

    public IQueryable<Event> FilterByAddressCity(IQueryable<Event> events, string city)
    {
        return events.Where(e => e.Address.City.Contains(city, StringComparison.CurrentCultureIgnoreCase));
    }

    public IQueryable<Event> FilterByAddressStreet(IQueryable<Event> events, string street, uint? houseNumber = null, uint? flatNumber = null)
    {
        var result = events.Where(e => e.Address.Street != null && e.Address.Street.Contains(street, StringComparison.CurrentCultureIgnoreCase));
        if (houseNumber != null)
        {
            result = result.Where(e => e.Address.HouseNumber != null && e.Address.HouseNumber == houseNumber);
        }
        if (flatNumber != null)
        {
            result = result.Where(e => e.Address.FlatNumber != null && e.Address.FlatNumber == flatNumber);
        }
        return result;
    }

    public IQueryable<Event> FilterByAddressPostalCode(IQueryable<Event> events, string postalCode)
    {
        return events.Where(e => e.Address.PostalCode == postalCode);
    }

    public IQueryable<Event> FilterByCategoriesNames(IQueryable<Event> events, IEnumerable<string> categoriesNames)
    {
        return events.Where(e => e.Categories.Any(c => categoriesNames.Contains(c.Name)));
    }
}
