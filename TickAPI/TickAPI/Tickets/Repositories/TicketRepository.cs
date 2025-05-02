using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
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

    public async Task<Result<bool>> CheckIfTicketBelongsToCustomerAsync(Guid id, string email)
    {
        var count = await _tickApiDbContext.Tickets.Join(_tickApiDbContext.Customers,
            ticket => ticket.Owner.Id, customer => customer.Id, (ticket, customer) => new {ticket.Id}).CountAsync();
        
        if (count > 0)
        {
            return Result<bool>.Success(true);
        }

        return Result<bool>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist " +
                                                                       "for this user");
    }

    public async Task<Result<Ticket>> GetTicketByIdAsync(Guid id)
    {
        var ticket = await _tickApiDbContext.Tickets.Include(t => t.Type).Include(t => t.Type.Event)
            .Include(t => t.Type.Event.Organizer).FirstOrDefaultAsync();
        if (ticket == null)
        {
            return Result<Ticket>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist");
        }
        return Result<Ticket>.Success(ticket);
    }
}