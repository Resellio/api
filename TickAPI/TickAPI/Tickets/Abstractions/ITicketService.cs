using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketService
{
    public Result<uint> GetNumberOfAvailableTicketsByType(TicketType ticketType);
    public Task<Result<GetTicketDetailsResponseDto>> GetTicketDetailsAsync(string email, Guid ticketGuid);
}