using TickAPI.Common.Results;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }
    
    public async Task<Result> AddNewTicketAsync(Guid ticketTypeId, string customerEmail, string? nameOnTicket, string? seats)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        cart.Tickets.Add(new ShoppingCartNewTicket()
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

    public Task<Result> GetTicketsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result> RemoveTicketAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result> CheckoutAsync()
    {
        throw new NotImplementedException();
    }
}