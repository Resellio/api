namespace TickAPI.TicketTypes.DTOs.Request;

public record CreateEventTicketTypeDto(
    string Description,
    uint MaxCount,
    decimal Price,
    string Currency,
    DateTime AvailableFrom
    );