using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketFilterApplier
{
    public IQueryable<Ticket> ApplyFilters(TicketFiltersDto filters);
}