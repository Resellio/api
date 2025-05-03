using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Models;

public class ShoppingCart
{
    public List<ShoppingCartNewTicket> Tickets { get; set; } = [];
    public List<ShoppingCartResellTicket> ResellTickets { get; set; } = [];
}