using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.DTOs.Response;

public record GetShoppingCartTicketsResponseDto(
    List<ShoppingCartNewTicket> NewTickets,
    List<ShoppingCartResellTicket> ResellTickets
);