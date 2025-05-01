namespace TickAPI.Events.DTOs.Response;

public record GetEventDetailsResponseTicketTypeDto(
    Guid Id,
    string Description,
    decimal Price,
    string Currency,
    DateTime AvailableFrom,
    uint AmountAvailable
);