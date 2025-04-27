using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketRepository
{
    public IQueryable<Ticket> GetAllTicketsByTicketType(TicketType ticketType);
}