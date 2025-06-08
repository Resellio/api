using TickAPI.Events.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventFilter
{
    IQueryable<Event> GetEvents();
    void FilterByNameOrDescription(string name);
    void FilterByStartDate(DateTime startDate);
    void FilterByMinStartDate(DateTime startDate);
    void FilterByMaxStartDate(DateTime startDate);
    void FilterByEndDate(DateTime endDate);
    void FilterByMinEndDate(DateTime endDate);
    void FilterByMaxEndDate(DateTime endDate);
    void FilterByMinPrice(decimal minPrice);
    void FilterByMaxPrice(decimal maxPrice);
    void FilterByMinAge(uint minAge);
    void FilterByMaxMinimumAge(uint maxMinimumAge);
    void FilterByAddressCountry(string country);
    void FilterByAddressCity(string city);
    void FilterByAddressStreet(string street, uint? houseNumber, uint? flatNumber);
    void FilterByAddressPostalCode(string postalCode);
    void FilterByCategoriesNames(IEnumerable<string> categoriesNames);
}
