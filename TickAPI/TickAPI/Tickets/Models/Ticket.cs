using TickAPI.Customers.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Models;

public class Ticket
{
    public Guid Id  { get; set; }
    public TicketType Type { get; set; }
    public Customer Owner { get; set; }
    public String NameOnTicket { get; set; }
    public String Seats { get; set; }
    public bool ForResell { get; set; }
}