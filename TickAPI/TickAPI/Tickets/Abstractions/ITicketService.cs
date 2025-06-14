using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Models;
using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Abstractions;

public interface ITicketService
{
    public Task<Result<uint>> GetNumberOfAvailableTicketsByTypeAsync(TicketType ticketType);
    public Task<Result<uint>> GetNumberOfAvailableTicketsByTypeIdAsync(Guid ticketTypeId);
    public Task<Result<bool>> CheckTicketAvailabilityByTypeIdAsync(Guid ticketTypeId, uint amount);
    public Task<Result<PaginatedData<GetTicketForResellResponseDto>>> GetTicketsForResellAsync(Guid eventId, int page,
        int pageSize);
    public Task<Result<PaginatedData<GetTicketForCustomerDto>>> GetTicketsForCustomerAsync(string email, int page,
        int pageSize, TicketFiltersDto ? ticketFilters = null);
    public Task<Result> ScanTicket(Guid ticketGuid);
    public Task<Result<GetTicketDetailsResponseDto>> GetTicketDetailsAsync(Guid ticketGuid, string email,
        string scanUrl);
    public Task<Result> SetTicketForResellAsync(Guid ticketId, string email, decimal resellPrice, string resellCurrency);
    public Task<Result<Ticket>> GetTicketByIdAsync(Guid ticketId);
    public Task<Result<TicketType>> GetTicketTypeByIdAsync(Guid ticketTypeId);
    public Task<Result<Ticket>> CreateTicketAsync(TicketType type, Customer owner, string? nameOnTicket = null,
        string? seats = null);
    public Task<Result> ChangeTicketOwnershipViaResellAsync(Ticket ticket, Customer newOwner, string? nameOnTicket = null);
}