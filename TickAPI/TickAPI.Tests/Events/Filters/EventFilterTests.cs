using TickAPI.Addresses.Models;
using TickAPI.Categories.Models;
using TickAPI.Events.Filters;
using TickAPI.Events.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Events.Filters;

public class EventFilterTests
{
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
            },
            Categories = new List<Category>
            {
                new Category { Name = "Music" },
                new Category { Name = "Rock" }
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
            },
            Categories = new List<Category>
            {
                new Category { Name = "Music" },
                new Category { Name = "Jazz" }
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
            },
            Categories = new List<Category>
            {
                new Category { Name = "Technology" },
                new Category { Name = "Development" }
            }
        }
    ];

    [Fact]
    public void FilterByName_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByName("concert");
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByDescription("tech");
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByStartDate(new DateTime(2025, 5, 1));
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }
    
    [Fact]
    public void FilterByMinStartDate_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMinStartDate(new DateTime(2025, 5, 15));
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[1], result);
    }
    
    [Fact]
    public void FilterByMaxStartDate_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMaxStartDate(new DateTime(2025, 5, 15));
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByEndDate(new DateTime(2025, 5, 1, 22, 0, 0));
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[0], result);
    }
    
    [Fact]
    public void FilterByMinEndDate_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMinEndDate(new DateTime(2025, 5, 15));
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[1], result);
    }
    
    [Fact]
    public void FilterByMaxEndDate_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMaxEndDate(new DateTime(2025, 5, 15));
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }
    
    [Fact]
    public void FilterByMinPrice_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMinPrice(100);
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMaxPrice(100);
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[1], result);
    }
    
    [Fact]
    public void FilterByMinAge_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMinAge(18);
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[0], result);
        Assert.Contains(events[2], result);
    }

    [Fact]
    public void FilterByMaxMinimumAge_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByMaxMinimumAge(18);
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByAddressCountry("poland");
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByAddressCity("berlin");
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByAddressStreet("marszałkowska", 12, 5);
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByAddressStreet("marszałkowska", 12);
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByAddressStreet("długa");
        var result = eventFilter.GetEvents().ToList();

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
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByAddressPostalCode("10117");
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(events[1], result);
    }

    [Fact]
    public void FilterByCategoriesNames_ShouldReturnMatchingEvents()
    {
        // Arrange
        var events = GetTestEvents();

        // Act
        var eventFilter = new EventFilter(events.AsQueryable());
        eventFilter.FilterByCategoriesNames(["Jazz", "Technology"]);
        var result = eventFilter.GetEvents().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(events[1], result);
        Assert.Contains(events[2], result);
    }

}
