using TickAPI.Events.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.TicketTypes.Models;

public class TicketType
{
    public Guid Id { get; set; }
    public Event Event { get; set; }
    public string Description { get; set; }
    public uint MaxCount { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public DateTime AvailableFrom { get; set; }
    public ICollection<Ticket> Tickets { get; set; } 
}