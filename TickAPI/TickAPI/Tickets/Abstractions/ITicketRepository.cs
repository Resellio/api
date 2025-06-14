using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Models;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketRepository
{
    public IQueryable<Ticket> GetAllTicketsByTicketType(TicketType ticketType);
    public Task<Result<Ticket>> GetTicketWithDetailsByIdAndEmailAsync(Guid id, string email);
    public IQueryable<Ticket> GetTicketsByEventId(Guid eventId);
    public IQueryable<Ticket> GetTicketsByCustomerEmail(string email);
    public Task<Result> MarkTicketAsUsed(Guid id);
    public Task<Result> SetTicketForResell(Guid ticketId, decimal newPrice, string currency);
    public Task<Result<Ticket>> AddTicketAsync(Ticket ticket);
    public Task<Result<Ticket>> GetTicketWithDetailsByIdAsync(Guid id);
    public Task<Result> ChangeTicketOwnershipAsync(Ticket ticket, Customer newOwner, string? nameOnTicket = null);
}