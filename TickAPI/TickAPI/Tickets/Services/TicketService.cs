using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPaginationService _paginationService;

    public TicketService(ITicketRepository ticketRepository, IPaginationService paginationService)
    {
        _ticketRepository = ticketRepository;
        _paginationService = paginationService;
    }
    
    // TODO: Update this method to also count tickets cached in Redis as unavailable
    public Result<uint> GetNumberOfAvailableTicketsByType(TicketType ticketType)
    {
        var unavailableTickets = _ticketRepository.GetAllTicketsByTicketType(ticketType);
        
        var availableCount = ticketType.MaxCount - unavailableTickets.Count();

        if (availableCount < 0)
        {
            return Result<uint>.Failure(StatusCodes.Status500InternalServerError,
                "The number of available tickets is negative.");
        }
        
        return Result<uint>.Success((uint)availableCount);
    }

    public async Task<Result<PaginatedData<GetTicketForResellResponseDto>>> GetTicketsForResellAsync(Guid eventId, int page, int pageSize)
    {
        var eventTickets = _ticketRepository.GetTicketsByEventId(eventId);
        var ticketsForResell = eventTickets.Where(t => t.ForResell);
        var paginatedTicketsResult = await _paginationService.PaginateAsync(ticketsForResell, pageSize, page);
        if (paginatedTicketsResult.IsError)
        {
            return Result<PaginatedData<GetTicketForResellResponseDto>>.PropagateError(paginatedTicketsResult);
        }
        var paginatedResult = _paginationService.MapData(paginatedTicketsResult.Value!,
            t => new GetTicketForResellResponseDto(t.Id, t.Type.Price, t.Type.Currency, t.Type.Description, t.Seats));
        return Result<PaginatedData<GetTicketForResellResponseDto>>.Success(paginatedResult);
    }
    //TODO: Maybe apply some filtering over here?
    public async Task<Result<PaginatedData<GetTicketForCustomerDto>>> GetTicketsForCustomerAsync(string email, int page, int pageSize)
    {
        var customerTickets = _ticketRepository.GetTicketsByCustomerEmail(email);
        var paginatedCustomerTickets = await _paginationService.PaginateAsync(customerTickets, pageSize, page);
        if (paginatedCustomerTickets.IsError)
        {
            return Result<PaginatedData<GetTicketForCustomerDto>>.PropagateError(paginatedCustomerTickets);
        }

        var paginatedResult = _paginationService.MapData(paginatedCustomerTickets.Value!,
            t => new GetTicketForCustomerDto(t.Type.Event.Name, t.Type.Event.StartDate, t.Type.Event.EndDate));
        
        return Result<PaginatedData<GetTicketForCustomerDto>>.Success(paginatedResult);
    }

    public async Task<Result<GetTicketDetailsResponseDto>> GetTicketDetailsAsync(Guid ticketGuid, string email)
    {
        var ticketRes = await _ticketRepository.GetTicketWithDetailsByIdAndEmailAsync(ticketGuid, email);
        if (ticketRes.IsError)
        {
            return Result<GetTicketDetailsResponseDto>.PropagateError(ticketRes);
        }
        var ticket = ticketRes.Value!;
        var ev = ticket.Type.Event;
        var address = new GetTicketDetailsAddressDto(ev.Address.Country, ev.Address.City, ev.Address.PostalCode,
            ev.Address.Street, ev.Address.HouseNumber, ev.Address.FlatNumber);
        var ticketDetails = new GetTicketDetailsResponseDto
        (
            ticket.NameOnTicket,
            ticket.Seats,
            ticket.Type.Price,
            ticket.Type.Currency,
            ticket.Type.Event.Name,
            ticket.Type.Event.Organizer.DisplayName, 
            ticket.Type.Event.StartDate,
            ticket.Type.Event.EndDate,
            address,
            ticket.Type.Event.Id
        );
        return  Result<GetTicketDetailsResponseDto>.Success(ticketDetails);
    }
}