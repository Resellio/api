using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketRepository
{
    public IQueryable<Ticket> GetAllTicketsByTicketType(TicketType ticketType);
    public Task<Result<Ticket>> GetTicketWithDetailsByIdAndEmailAsync(Guid id, string email);
    public IQueryable<Ticket> GetTicketsByEventId(Guid eventId);
}