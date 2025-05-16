namespace TickAPI.ShoppingCarts.DTOs.Response;

public record GetShoppingCartTicketsResellTicketDetailsResponseDto(
    Guid TicketId,
    string EventName,
    string TicketType,
    string OrganizerName,
    string OriginalOwnerEmail
);