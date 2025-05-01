using TickAPI.Common.Results.Generic;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketService
{
    public Result<uint> GetNumberOfAvailableTicketsByType(TicketType ticketType);
}