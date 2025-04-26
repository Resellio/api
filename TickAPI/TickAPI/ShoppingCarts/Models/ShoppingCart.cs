using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Models;

public class ShoppingCart
{
    public List<Ticket> Tickets { get; set; }
}