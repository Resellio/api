using TickAPI.Addresses.Models;

namespace TickAPI.Tickets.DTOs.Response;

public record GetTicketDetailsResponseDto
(
    string NameOnTicket,
    string? Seats,
    decimal Price,
    string Currency,
    string EventName,
    string OrganizerName,
    DateTime StartDate,
    DateTime EndDate,
    GetTicketDetailsAddressDto Address,
    Guid eventId,
    string qrcode
);