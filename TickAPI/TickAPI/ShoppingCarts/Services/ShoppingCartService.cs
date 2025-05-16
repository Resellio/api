using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.Models;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Response;
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
        if (amount <= 0)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "amount of bought tickets must be greater than 0");
        }
        
        var availabilityResult = _ticketService.CheckTicketAvailabilityByTypeId(ticketTypeId, amount);

        if (availabilityResult.IsError)
        {
            return Result.PropagateError(availabilityResult);
        }

        if (!availabilityResult.Value)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, $"not enough available tickets of type {ticketTypeId}");
        }
        
        var addTicketToCartResult = await _shoppingCartRepository.AddNewTicketToCartAsync(customerEmail, ticketTypeId, amount);

        if (addTicketToCartResult.IsError)
        {
            return Result.PropagateError(addTicketToCartResult);
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
        var result = new GetShoppingCartTicketsResponseDto(cart.NewTickets, cart.ResellTickets);
        
        return Result<GetShoppingCartTicketsResponseDto>.Success(result);
    }

    public Task<Result> RemoveNewTicketsFromCartAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result> CheckoutAsync()
    {
        throw new NotImplementedException();
    }
}