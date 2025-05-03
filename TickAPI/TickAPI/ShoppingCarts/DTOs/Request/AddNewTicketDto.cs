namespace TickAPI.ShoppingCarts.DTOs.Request;

public record AddNewTicketDto(
    Guid TicketTypeId,
    string? NameOnTicket,
    string? Seats
);