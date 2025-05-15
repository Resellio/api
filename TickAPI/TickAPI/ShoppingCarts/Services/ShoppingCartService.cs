using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Response;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }
    
    public async Task<Result> AddNewTicketToCartAsync(Guid ticketTypeId, string customerEmail, string? nameOnTicket, string? seats)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        cart.NewTickets.Add(new ShoppingCartNewTicket()
        {
            TicketTypeId = ticketTypeId,
            NameOnTicket = nameOnTicket,
            Seats = seats,
        });
        
        var updateShoppingCartResult = await _shoppingCartRepository.UpdateShoppingCartAsync(customerEmail, cart);

        if (updateShoppingCartResult.IsError)
        {
            return Result.PropagateError(updateShoppingCartResult);
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

    public Task<Result> RemoveTicketFromCartAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result> CheckoutAsync()
    {
        throw new NotImplementedException();
    }
}