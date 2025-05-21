namespace TickAPI.ShoppingCarts.DTOs.Response;

public record GetShoppingCartTicketsNewTicketDetailsResponseDto(
    Guid TicketTypeId,
    string EventName,
    string TicketType,
    string OrganizerName,
    uint Quantity,
    decimal UnitPrice
);