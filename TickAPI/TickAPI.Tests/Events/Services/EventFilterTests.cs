using TickAPI.Addresses.Models;
using TickAPI.Events.Models;
using TickAPI.Events.Services;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Events.Services;

public class EventFilterTests
{
    private readonly EventFilter _eventFilter = new EventFilter();

    private static List<Event> GetTestEvents() =>
    [
        new Event
        {
            Name = "Concert A",
            Description = "An amazing rock concert",
            StartDate = new DateTime(2025, 5, 1, 18, 0, 0),
            EndDate = new DateTime(2025, 5, 1, 22, 0, 0),
            MinimumAge = 18,
            Address = new Address
            {
                Country = "Poland",
                City = "Warsaw",
                Street = "Marszałkowska",
                HouseNumber = 12,
                FlatNumber = 5,
                PostalCode = "00-001"
            },
            TicketTypes = new List<TicketType>
            {
                new TicketType { Price = 100 },
                new TicketType { Price = 120 }
            }
        },

        new Event
        {
            Name = "Concert B",
            Description = "Chill jazz night",
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2025, 6, 2),
            MinimumAge = 12,
            Address = new Address
            {
                Country = "Germany",
                City = "Berlin",
                Street = "Unter den Linden",
                HouseNumber = 44,
                FlatNumber = 10,
                PostalCode = "10117"
            },
            TicketTypes = new List<TicketType>
            {
                new TicketType { Price = 50 }
            }
        },

        new Event
        {
            Name = "Conference",
            Description = "Tech event for developers",
            StartDate = new DateTime(2025, 5, 1),
            EndDate = new DateTime(2025, 5, 3),
            MinimumAge = 21,
            Address = new Address
            {
                Country = "Poland",
                City = "Krakow",
                Street = "Długa",
                HouseNumber = 7,
                FlatNumber = 3,
                PostalCode = "31-147"
            },
            TicketTypes = new List<TicketType>
            {
                new TicketType { Price = 200 },
                new TicketType { Price = 150 }
            }
        }
    ];

    [Fact]
    public void FilterByName_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByName(events.AsQueryable(), "concert").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[1], result);
    }
    
    [Fact]
    public void FilterByDescription_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByDescription(events.AsQueryable(), "tech").ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[2], result);
    }
    
    [Fact]
    public void FilterByStartDate_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByStartDate(events.AsQueryable(), new DateTime(2025, 5, 1)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }
    
    [Fact]
    public void FilterByEndDate_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByEndDate(events.AsQueryable(), new DateTime(2025, 5, 1, 22, 0, 0)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[0], result);
    }
    
    [Fact]
    public void FilterByMinPrice_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByMinPrice(events.AsQueryable(), 100).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }

    [Fact]
    public void FilterByMaxPrice_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByMaxPrice(events.AsQueryable(), 100).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[1], result);
    }
    
    [Fact]
    public void FilterByMinAge_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByMinAge(events.AsQueryable(), 18).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }

    [Fact]
    public void FilterByMaxAge_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByMaxAge(events.AsQueryable(), 18).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[1], result);
    }
    
    [Fact]
    public void FilterByAddressCountry_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByAddressCountry(events.AsQueryable(), "poland").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }

    [Fact]
    public void FilterByAddressCity_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByAddressCity(events.AsQueryable(), "berlin").ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[1], result);
    }

    [Fact]
    public void FilterByAddressStreet_WithAllParameters_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByAddressStreet(events.AsQueryable(), "marszałkowska", 12, 5).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[0], result);
    }

    [Fact]
    public void FilterByAddressStreet_WithoutFlatNumber_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByAddressStreet(events.AsQueryable(), "marszałkowska", 12).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[0], result);
    }

    [Fact]
    public void FilterByAddressStreet_WithStreetOnly_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByAddressStreet(events.AsQueryable(), "długa").ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[2], result);
    }

    [Fact]
    public void FilterByAddressPostalCode_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var result = _eventFilter.FilterByAddressPostalCode(events.AsQueryable(), "10117").ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[1], result);
    }

}
