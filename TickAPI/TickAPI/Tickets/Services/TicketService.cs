using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.TicketTypes.Models;


namespace TickAPI.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
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

    public async Task<Result<GetTicketDetailsResponseDto>> GetTicketDetailsAsync(string email, Guid ticketGuid)
    {
        var exists = await _ticketRepository.CheckIfTicketBelongsToCustomerAsync(ticketGuid, email);
        if (!exists.IsSuccess)
        {
            return Result<GetTicketDetailsResponseDto>.PropagateError(exists);
        }

        var ticketRes = await _ticketRepository.GetTicketByIdAsync(ticketGuid);
        if (!ticketRes.IsSuccess)
        {
            return Result<GetTicketDetailsResponseDto>.PropagateError(ticketRes);
        }
        var ticket = ticketRes.Value;
        var ticketDetails = new GetTicketDetailsResponseDto
        {
            NameOnTicket = ticket.NameOnTicket,
            Seats = ticket.Seats,
            Price = ticket.Type.Price,
            Currency = ticket.Type.Currency,
            EventName = ticket.Type.Event.Name,
            OrganizerName = ticket.Type.Event.Organizer.DisplayName,
            StartDate = ticket.Type.Event.StartDate,
            EndDate = ticket.Type.Event.EndDate,
        };
        
        return  Result<GetTicketDetailsResponseDto>.Success(ticketDetails);

    }
}