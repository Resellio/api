using TickAPI.Events.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventFilter
{
    IQueryable<Event> GetEvents();
    void FilterByName(string name);
    void FilterByDescription(string description);
    void FilterByStartDate(DateTime startDate);
    void FilterByEndDate(DateTime endDate);
    void FilterByMinPrice(decimal minPrice);
    void FilterByMaxPrice(decimal maxPrice);
    void FilterByMinAge(uint minAge);
    void FilterByMaxAge(uint maxAge);
    void FilterByAddressCountry(string country);
    void FilterByAddressCity(string city);
    void FilterByAddressStreet(string street, uint? houseNumber, uint? flatNumber);
    void FilterByAddressPostalCode(string postalCode);
    void FilterByCategoriesNames(IEnumerable<string> categoriesNames);
}
