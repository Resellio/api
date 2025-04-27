namespace TickAPI.Tickets.Models;

public class ShoppingCartTicket
{
    public Guid TicketTypeId { get; set; }
    public Guid CustomerId { get; set; }
    public string NameOnTicket { get; set; }
    public string? Seats { get; set; }
}