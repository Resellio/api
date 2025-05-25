namespace TickAPI.Tickets.Models;

public class ShoppingCartNewTicket
{
    public Guid TicketTypeId { get; set; }
    public uint Quantity { get; set; }
}