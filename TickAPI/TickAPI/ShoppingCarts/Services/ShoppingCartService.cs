using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.Models;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Response;
using TickAPI.ShoppingCarts.Mappers;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly ITicketService _ticketService;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, ITicketService ticketService)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _ticketService = ticketService;
    }
    
    public async Task<Result> AddNewTicketsToCartAsync(Guid ticketTypeId, uint amount, string customerEmail)
    {
        var availabilityResult = await _ticketService.CheckTicketAvailabilityByTypeIdAsync(ticketTypeId, amount);

        if (availabilityResult.IsError)
        {
            return Result.PropagateError(availabilityResult);
        }

        if (!availabilityResult.Value)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, $"not enough available tickets of type {ticketTypeId}");
        }
        
        var addTicketsToCartResult = await _shoppingCartRepository.AddNewTicketsToCartAsync(customerEmail, ticketTypeId, amount);

        if (addTicketsToCartResult.IsError)
        {
            return Result.PropagateError(addTicketsToCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsFromCartAsync(string customerEmail)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result<GetShoppingCartTicketsResponseDto>.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var newTickets = new List<GetShoppingCartTicketsNewTicketDetailsResponseDto>();

        foreach (var ticket in cart.NewTickets)
        {
            var newTicketResult = await _ticketService.GetTicketTypeByIdAsync(ticket.TicketTypeId);

            if (newTicketResult.IsError)
            {
                return Result<GetShoppingCartTicketsResponseDto>.PropagateError(newTicketResult);
            }

            var newTicket =
                ShoppingCartMapper.MapTicketTypeToGetShoppingCartTicketsNewTicketDetailsResponseDto(
                    newTicketResult.Value!, ticket.Quantity);
            
            newTickets.Add(newTicket);
        }
        
        var result = new GetShoppingCartTicketsResponseDto(newTickets, []);
        
        return Result<GetShoppingCartTicketsResponseDto>.Success(result);
    }

    public async Task<Result> RemoveNewTicketsFromCartAsync(Guid ticketTypeId, uint amount, string customerEmail)
    {
        var removeTicketsFromCartResult = await _shoppingCartRepository.RemoveNewTicketsFromCartAsync(customerEmail, ticketTypeId, amount);

        if (removeTicketsFromCartResult.IsError)
        {
            return Result.PropagateError(removeTicketsFromCartResult);
        }
        
        return Result.Success();
    }

    public Task<Result> CheckoutAsync()
    {
        throw new NotImplementedException();
    }
}