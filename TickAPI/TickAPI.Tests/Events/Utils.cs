using TickAPI.Addresses.Models;
using TickAPI.Categories.Models;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Events;

public static class Utils
{
    public static Event CreateSampleEvent(string name)
    {
        return new Event
        {
            Id = Guid.Parse("c5aa4979-af8c-4cf9-a827-b273317fbc70"),
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
            },
            TicketTypes = new List<TicketType>
            {
                new TicketType {Id = Guid.Parse("b2ad06a4-aaff-4cfb-92af-07c971e9aa3b"), Description = "Description #1", Price = 100, Currency = "PLN", AvailableFrom = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)},
                new TicketType {Id = Guid.Parse("7ecfc61a-32d2-4124-a95c-cb5834a49990"), Description = "Description #2", Price = 300, Currency = "PLN", AvailableFrom = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)},
                new TicketType {Id = Guid.Parse("7be2ae57-2394-4854-bf11-9567ce7e0ab6"), Description = "Description #3", Price = 200, Currency = "PLN", AvailableFrom = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)},
            }
        };
    }

    public static GetEventResponseDto CreateSampleEventResponseDto(string name)
    {
        return new GetEventResponseDto(
            Guid.Parse("c5aa4979-af8c-4cf9-a827-b273317fbc70"),
            name,
            $"Description of {name}",
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            18,
            new GetEventResponsePriceInfoDto(100, "PLN"),
            new GetEventResponsePriceInfoDto(300, "PLN"),
            [new GetEventResponseCategoryDto("Test")],
            EventStatus.TicketsAvailable,
            new GetEventResponseAddressDto("United States", "New York", "10001", "Main St", 123, null)
        );
    }

    public static GetEventDetailsResponseDto CreateSampleEventDetailsDto(string name)
    {
        return new GetEventDetailsResponseDto(
            Guid.Parse("c5aa4979-af8c-4cf9-a827-b273317fbc70"),
            name,
            $"Description of {name}",
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            18,
            [new GetEventResponseCategoryDto("Test")],
            [new GetEventDetailsResponseTicketTypeDto(Guid.Parse("b2ad06a4-aaff-4cfb-92af-07c971e9aa3b"), "Description #1", 100, "PLN", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10),
                new GetEventDetailsResponseTicketTypeDto(Guid.Parse("7ecfc61a-32d2-4124-a95c-cb5834a49990"), "Description #2", 300, "PLN", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), 30),
                new GetEventDetailsResponseTicketTypeDto(Guid.Parse("7be2ae57-2394-4854-bf11-9567ce7e0ab6"), "Description #3", 200, "PLN", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), 20)],
            EventStatus.TicketsAvailable,
            new GetEventResponseAddressDto("United States", "New York", "10001", "Main St", 123, null)
        );
    }
}
