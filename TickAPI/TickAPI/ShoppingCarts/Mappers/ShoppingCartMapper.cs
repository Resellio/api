using TickAPI.ShoppingCarts.DTOs.Response;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.ShoppingCarts.Mappers;

public static class ShoppingCartMapper
{
    public static GetShoppingCartTicketsNewTicketDetailsResponseDto
        MapTicketTypeToGetShoppingCartTicketsNewTicketDetailsResponseDto(TicketType type, uint quantity)
    {
        return new GetShoppingCartTicketsNewTicketDetailsResponseDto(
            type.Id,
            type.Event.Name,
            type.Description,
            type.Event.Organizer.DisplayName,
            quantity,
            type.Price,
            type.Currency
        );
    }

    public static GetShoppingCartTicketsResellTicketDetailsResponseDto
        MapTicketToGetShoppingCartTicketsResellTicketDetailsResponseDto(Ticket ticket)
    {
        return new GetShoppingCartTicketsResellTicketDetailsResponseDto(
            ticket.Id,
            ticket.Type.Event.Name,
            ticket.Type.Description,
            ticket.Type.Event.Organizer.DisplayName,
            ticket.Owner.Email,
            ticket.Type.Price,
            ticket.Type.Currency
        );
    }
}