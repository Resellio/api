namespace TickAPI.ShoppingCarts.DTOs.Response;

public record GetShoppingCartTicketsResponseDto(
    List<GetShoppingCartTicketsNewTicketDetailsResponseDto> NewTickets,
    List<GetShoppingCartTicketsResellTicketDetailsResponseDto> ResellTickets
);