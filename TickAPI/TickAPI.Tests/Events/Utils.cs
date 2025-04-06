using TickAPI.Addresses.Models;
using TickAPI.Categories.Models;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.Models;

namespace TickAPI.Tests.Events;

public static class Utils
{
    public static Event CreateSampleEvent(string name)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description of {name}",
            StartDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            MinimumAge = 18,
            EventStatus = EventStatus.TicketsAvailable,
            Categories = new List<Category> { new Category { Name = "Test" } },
            Address = new Address
            {
                Country = "United States",
                City = "New York",
                PostalCode = "10001",
                Street = "Main St",
                HouseNumber = 123,
                FlatNumber = null
            }
        };
    }

    public static GetEventResponseDto CreateSampleEventResponseDto(string name)
    {
        return new GetEventResponseDto(
            name,
            $"Description of {name}",
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            18,
            [new GetEventResponseCategoryDto("Test")],
            EventStatus.TicketsAvailable,
            new GetEventResponseAddressDto("United States", "New York", "10001", "Main St", 123, null)
        );
    }
}
