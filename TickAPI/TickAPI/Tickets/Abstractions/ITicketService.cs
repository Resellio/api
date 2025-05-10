using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketService
{
    public Result<uint> GetNumberOfAvailableTicketsByType(TicketType ticketType);
    public Task<Result<PaginatedData<GetTicketForResellResponseDto>>> GetTicketsForResellAsync(Guid eventId, int page,
        int pageSize);
    public Task<Result<PaginatedData<GetTicketForCustomerDto>>> GetTicketsForCustomerAsync(string email, int page,
        int pageSize, TicketFiltersDto ? ticketFilters = null);
    public Task<Result<bool>> ScanTicket(Guid ticketGuid);

    public Task<Result<GetTicketDetailsResponseDto>> GetTicketDetailsAsync(Guid ticketGuid, string email,
        string scanUrl);
}