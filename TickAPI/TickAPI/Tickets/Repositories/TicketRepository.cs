using Microsoft.EntityFrameworkCore;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public TicketRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }
    
    public IQueryable<Ticket> GetAllTicketsByTicketType(TicketType ticketType)
    {
        return _tickApiDbContext.Tickets.Where(t => t.Type == ticketType);
    }

    public IQueryable<Ticket> GetTicketsByEventId(Guid eventId)
    {
        return _tickApiDbContext.Tickets
            .Include(t => t.Type)
            .Include(t => t.Type.Event)
            .Where(t => t.Type.Event.Id == eventId);
    }
}