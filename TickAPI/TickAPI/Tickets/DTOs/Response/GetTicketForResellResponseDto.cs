namespace TickAPI.Tickets.DTOs.Response;

public record GetTicketForResellResponseDto(
    Guid Id,
    decimal Price,
    string Currency,
    string Description,
    string? Seats
);
