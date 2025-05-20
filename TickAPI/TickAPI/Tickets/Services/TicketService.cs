using Azure.Core;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.QR.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Tickets.Filters;
using TickAPI.TicketTypes.Abstractions;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly IPaginationService _paginationService;
    private readonly IQRCodeService _qrCodeService;
    public TicketService(ITicketRepository ticketRepository, ITicketTypeRepository ticketTypeRepository,
        IPaginationService paginationService, IQRCodeService qrCodeService)
    {
        _ticketRepository = ticketRepository;
        _ticketTypeRepository = ticketTypeRepository;
        _paginationService = paginationService;
        _qrCodeService = qrCodeService;
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

    public async Task<Result<uint>> GetNumberOfAvailableTicketsByTypeIdAsync(Guid ticketTypeId)
    {
        var ticketTypeResult = await _ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);

        if (ticketTypeResult.IsError)
        {
            return Result<uint>.PropagateError(ticketTypeResult);
        }
        
        return GetNumberOfAvailableTicketsByType(ticketTypeResult.Value!);
    }

    public async Task<Result<bool>> CheckTicketAvailabilityByTypeIdAsync(Guid ticketTypeId, uint amount)
    {
        var numberOfTicketsResult = await GetNumberOfAvailableTicketsByTypeIdAsync(ticketTypeId);

        if (numberOfTicketsResult.IsError)
        {
            return Result<bool>.PropagateError(numberOfTicketsResult);
        }
        
        var availableAmount = numberOfTicketsResult.Value!;

        return availableAmount >= amount ? Result<bool>.Success(true) : Result<bool>.Success(false);
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
    public async Task<Result<PaginatedData<GetTicketForCustomerDto>>> GetTicketsForCustomerAsync(string email, int page, int pageSize, TicketFiltersDto ? ticketFilters = null)
    {
        var customerTickets = _ticketRepository.GetTicketsByCustomerEmail(email);
        if (ticketFilters != null)
        {
            var filter = new TicketFilter(customerTickets);
            var applier = new  TicketFilterApplier(filter);
            customerTickets = applier.ApplyFilters(ticketFilters);   
        }
        var paginatedCustomerTickets = await _paginationService.PaginateAsync(customerTickets, pageSize, page);
        if (paginatedCustomerTickets.IsError)
        {
            return Result<PaginatedData<GetTicketForCustomerDto>>.PropagateError(paginatedCustomerTickets);
        }

        var paginatedResult = _paginationService.MapData(paginatedCustomerTickets.Value!,
            t => new GetTicketForCustomerDto(t.Id, t.Type.Event.Name, t.Type.Event.StartDate, t.Type.Event.EndDate, t.Used));
        
        return Result<PaginatedData<GetTicketForCustomerDto>>.Success(paginatedResult);
    }

    public async Task<Result<GetTicketDetailsResponseDto>> GetTicketDetailsAsync(Guid ticketGuid, string email, string scanUrl)
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
        
        var qrbytes = _qrCodeService.GenerateQrCode(scanUrl);
        var qrcode = Convert.ToBase64String(qrbytes);
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
            ticket.Type.Event.Id,
            qrcode
        );
        return  Result<GetTicketDetailsResponseDto>.Success(ticketDetails);
    }

    public async Task<Result<TicketType>> GetTicketTypeByIdAsync(Guid ticketTypeId)
    {
        var ticketTypeResult = await _ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);

        if (ticketTypeResult.IsError)
        {
            return Result<TicketType>.PropagateError(ticketTypeResult);
        }
        
        return Result<TicketType>.Success(ticketTypeResult.Value!);
    }

    public async Task<Result> ScanTicket(Guid ticketGuid)
    {
        var res = await _ticketRepository.MarkTicketAsUsed(ticketGuid);
        return res;
    }
    
    
}