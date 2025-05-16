namespace TickAPI.ShoppingCarts.DTOs.Response;

public record GetShoppingCartTicketsNewTicketDetailsResponseDto(
    Guid TicketId,
    string EventName,
    string TicketType,
    string OrganizerName,
    uint Quantity
);