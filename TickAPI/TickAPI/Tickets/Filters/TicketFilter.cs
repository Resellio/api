using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.Models;

namespace TickAPI.Tickets.Filters;

public class TicketFilter : ITicketFilter
{
    IQueryable<Ticket> _tickets;

    public TicketFilter(IQueryable<Ticket> tickets)
    {
        _tickets = tickets;
    }

    public IQueryable<Ticket> GetTickets()
    {
        return _tickets;
    }

    public void FilterUsedTickets()
    {
        _tickets = _tickets.Where(t => t.Used);
    }

    public void FilterUnusedTickets()
    {
        _tickets = _tickets.Where(t => !t.Used);
    }

    public void FilterTicketsForResell()
    {
        _tickets = _tickets.Where(t => t.ForResell);
    }

    public void FilterTicketsNotForResell()
    {
        _tickets = _tickets.Where(t => !t.ForResell);
    }

    public void FilterTicketsByEventName(string name)
    {
        _tickets = _tickets.Where(t => t.Type.Event.Name.ToLower().Contains(name.ToLower()));
    }
}