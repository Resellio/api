using TickAPI.Customers.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Models;

public class Ticket
{
    public Guid Id  { get; set; }
    public TicketType Type { get; set; }
    public Customer Owner { get; set; }
    public string NameOnTicket { get; set; }
    public string? Seats { get; set; }
    public bool ForResell { get; set; }
    public decimal? ResellPrice { get; set; }
    public bool Used { get; set; }
}