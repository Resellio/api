using Azure.Core;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.QR.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Tickets.Filters;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Abstractions;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IPaginationService _paginationService;
    private readonly IQRCodeService _qrCodeService;

    public TicketService(ITicketRepository ticketRepository, ITicketTypeRepository ticketTypeRepository, 
        IShoppingCartRepository shoppingCartRepository, IPaginationService paginationService, IQRCodeService qrCodeService)
    {
        _ticketRepository = ticketRepository;
        _ticketTypeRepository = ticketTypeRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _paginationService = paginationService;
        _qrCodeService = qrCodeService;
    }
    
    public async Task<Result<uint>> GetNumberOfAvailableTicketsByTypeAsync(TicketType ticketType)
    {
        var unavailableTickets = _ticketRepository.GetAllTicketsByTicketType(ticketType);
        var reservedTicketsAmountResult = await _shoppingCartRepository.GetAmountOfTicketTypeAsync(ticketType.Id);

        if (reservedTicketsAmountResult.IsError)
        {
            return Result<uint>.PropagateError(reservedTicketsAmountResult);
        }
        
        var reservedTicketsAmount = reservedTicketsAmountResult.Value;
        
        var availableCount = ticketType.MaxCount - unavailableTickets.Count() - reservedTicketsAmount;

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
        
        return await GetNumberOfAvailableTicketsByTypeAsync(ticketTypeResult.Value!);
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
            t =>
            {
                decimal price;
                string currency;
                if (t.ResellPrice is not null && t.ResellCurrency is not null)
                {
                    price = t.ResellPrice.Value;
                    currency = t.ResellCurrency;
                }
                else
                {
                    price = t.Type.Price;
                    currency = t.Type.Currency;
                }
                    
                return new GetTicketForResellResponseDto(t.Id, price, currency, t.Type.Description,
                    t.Seats);
            });
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

    public async Task<Result<Ticket>> GetTicketByIdAsync(Guid ticketId)
    {
        var ticketResult = await _ticketRepository.GetTicketWithDetailsByIdAsync(ticketId);

        if (ticketResult.IsError)
        {
            return Result<Ticket>.PropagateError(ticketResult);
        }
        
        return Result<Ticket>.Success(ticketResult.Value!);
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

    public async Task<Result> CreateTicketAsync(TicketType type, Customer owner, string? nameOnTicket = null,
        string? seats = null)
    {
        var ticket = new Ticket
        {
            Type = type,
            Owner = owner,
            NameOnTicket = nameOnTicket ?? owner.FirstName + " " + owner.LastName,
            Seats = seats,
            ForResell = false,
            Used = false,
        };
        
        var addTicketResult = await _ticketRepository.AddTicketAsync(ticket);

        return addTicketResult;
    }

    public async Task<Result> ChangeTicketOwnershipViaResellAsync(Ticket ticket, Customer newOwner, string? nameOnTicket = null)
    {
       var updateTicketResult = await _ticketRepository.ChangeTicketOwnershipAsync(ticket, newOwner);
       
       return updateTicketResult;
    }

    public async Task<Result> ScanTicket(Guid ticketGuid)
    {
        var res = await _ticketRepository.MarkTicketAsUsed(ticketGuid);
        return res;
    }

    public async Task<Result> SetTicketForResellAsync(Guid ticketId, string email, decimal resellPrice, string resellCurrency)
    {
        if (resellPrice <= 0)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "Price must be greater than zero");
        }
        var ticketRes = await _ticketRepository.GetTicketWithDetailsByIdAndEmailAsync(ticketId, email);
        if (ticketRes.IsError)
        {
            return Result.PropagateError(ticketRes);
        }

        if (ticketRes.Value!.Type.Price*1.5m < resellPrice)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "Resell price cannot exceed " +
                                                                            "value of original price times 1.5");
        }

        if (ticketRes.Value!.ForResell)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "Ticket is already set for resell");
        }

        if (ticketRes.Value!.Used)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "Ticket is already used");
        }
        
        var res = await _ticketRepository.SetTicketForResell(ticketId, resellPrice, resellCurrency);
        return res;
    }
}