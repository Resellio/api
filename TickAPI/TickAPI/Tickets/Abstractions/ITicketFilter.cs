using TickAPI.Tickets.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketFilter
{
    public IQueryable<Ticket> GetTickets();
    public void FilterUsedTickets();
    public void FilterUnusedTickets();
    public void FilterTicketsForResell();
    public void FilterTicketsNotForResell();
    public void FilterTicketsByEventName(string name);
}