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

    public IQueryable<Ticket> GetTicketsByEventId(Guid eventId)
    {
        return _tickApiDbContext.Tickets
            .Include(t => t.Type)
            .Include(t => t.Type.Event)
            .Where(t => t.Type.Event.Id == eventId);
    }

    public IQueryable<Ticket> GetTicketsByCustomerEmail(string email)
    {
        return _tickApiDbContext.Tickets
            .Include(t => t.Owner)
            .Include(t => t.Type)
            .Include(t =>t.Type.Event)
            .Where(t => t.Owner.Email == email);
    }
    
    public async Task<Result<Ticket>> GetTicketWithDetailsByIdAndEmailAsync(Guid id, string email)
    {
        var ticket = await _tickApiDbContext.Tickets.Include(t => t.Type).Include(t => t.Type.Event)
            .Include(t => t.Type.Event.Organizer).Include(t => t.Type.Event.Address)
            .Where(t => (t.Id == id && t.Owner.Email == email)).FirstOrDefaultAsync();
        if (ticket == null)
        {
            return Result<Ticket>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist");
        }
        return Result<Ticket>.Success(ticket);
    }

    public async Task<Result> MarkTicketAsUsed(Guid id)
    {
        var ticket = await _tickApiDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null || ticket.Used)
        {
            return Result.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist");
        }
        if (ticket.Used)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "Ticket already used");
        }
        ticket.Used = true;
        await _tickApiDbContext.SaveChangesAsync();
        return Result.Success();
    }
}