using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Customers.Models;
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
        var ticket = await _tickApiDbContext.Tickets
            .Include(t => t.Type)
            .Include(t => t.Type.Event)
            .Include(t => t.Type.Event.Organizer)
            .Include(t => t.Type.Event.Address)
            .Where(t => (t.Id == id && t.Owner.Email == email))
            .FirstOrDefaultAsync();
        if (ticket == null)
        {
            return Result<Ticket>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist");
        }
        return Result<Ticket>.Success(ticket);
    }

    public async Task<Result> MarkTicketAsUsed(Guid id)
    {
        var ticket = await _tickApiDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null)
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

    public async Task<Result> AddTicketAsync(Ticket ticket)
    {
        var maxCount = ticket.Type.MaxCount;

        if (maxCount <= _tickApiDbContext.Tickets.Count(t => t.Type.Id == ticket.Type.Id))
        {
            return Result.Failure(StatusCodes.Status400BadRequest,
                "The ticket you are trying to buy has already reached its max count");
        }
        
        _tickApiDbContext.Tickets.Add(ticket);
        await _tickApiDbContext.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Result<Ticket>> GetTicketWithDetailsByIdAsync(Guid id)
    {
        var ticket = await _tickApiDbContext.Tickets
            .Include(t => t.Type)
            .Include(t => t.Type.Event)
            .Include(t => t.Type.Event.Organizer)
            .Include(t => t.Type.Event.Address)
            .Include(t => t.Owner)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
        if (ticket == null)
        {
            return Result<Ticket>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist");
        }
        return Result<Ticket>.Success(ticket);
    }

    public async Task<Result> ChangeTicketOwnershipAsync(Ticket ticket, Customer newOwner, string? nameOnTicket = null)
    {
        var ticketFromDb = await _tickApiDbContext.Tickets
            .Include(t => t.Owner) // Include if needed
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        var newOwnerFromDb = await _tickApiDbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == newOwner.Id);

        if (ticketFromDb == null || newOwnerFromDb == null)
        {
            return Result.Failure(StatusCodes.Status404NotFound, "Ticket or new owner not found");
        }
        if (!ticketFromDb.ForResell)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "This ticket can't have its ownership passed");
        }
        if (ticketFromDb.Owner.Id == newOwnerFromDb.Id)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "You can't change the owner of the ticket to be the same");
        }

        ticketFromDb.Owner = newOwnerFromDb;
        ticketFromDb.NameOnTicket = nameOnTicket ?? $"{newOwnerFromDb.FirstName} {newOwnerFromDb.LastName}";

        await _tickApiDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> SetTicketForResell(Guid ticketId, decimal newPrice, string currency)
    {
        var ticket = await _tickApiDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return Result.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist");
        }
        ticket.ForResell = true;
        ticket.ResellCurrency = currency;
        ticket.ResellPrice = newPrice;
        await _tickApiDbContext.SaveChangesAsync();
        return Result.Success();
    }
}