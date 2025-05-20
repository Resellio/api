using TickAPI.Common.Redis.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Repositories;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly IRedisService _redisService;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(15);

    public ShoppingCartRepository(IRedisService redisService)
    {
        _redisService = redisService;
    }
    
    public async Task<Result<ShoppingCart>> GetShoppingCartByEmailAsync(string customerEmail)
    {
        var cartKey = GetCartKey(customerEmail);
        ShoppingCart? cart;
        
        try
        {
            cart = await _redisService.GetObjectAsync<ShoppingCart>(cartKey);
            await _redisService.KeyExpireAsync(cartKey, DefaultExpiry);
        }
        catch (Exception e)
        {
            return Result<ShoppingCart>.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        return Result<ShoppingCart>.Success(cart ?? new ShoppingCart());
    }

    public async Task<Result> UpdateShoppingCartAsync(string customerEmail, ShoppingCart shoppingCart)
    {
        var cartKey = GetCartKey(customerEmail);

        try
        {
            var res = await _redisService.SetObjectAsync(cartKey, shoppingCart, DefaultExpiry);
            if (!res)
            {
                return Result.Failure(StatusCodes.Status500InternalServerError, "the shopping cart could not be updated");
            }
        }
        catch (Exception e)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }
        
        return Result.Success();
    }

    public async Task<Result> AddNewTicketsToCartAsync(string customerEmail, Guid ticketTypeId, uint amount)
    {
        if (amount <= 0)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "amount of bought tickets must be greater than 0");
        }
        
        var getShoppingCartResult = await GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var existingEntry = cart.NewTickets.FirstOrDefault(t => t.TicketTypeId == ticketTypeId);

        if (existingEntry != null)
        {
            existingEntry.Quantity += amount;
        }
        else
        {
            cart.NewTickets.Add(new ShoppingCartNewTicket
            {
                TicketTypeId = ticketTypeId,
                Quantity = amount
            });
        }
        
        var updateShoppingCartResult = await UpdateShoppingCartAsync(customerEmail, cart);

        if (updateShoppingCartResult.IsError)
        {
            return Result.PropagateError(updateShoppingCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result> RemoveNewTicketsFromCartAsync(string customerEmail, Guid ticketTypeId, uint amount)
    {
        if (amount <= 0)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, "amount of removed tickets must be greater than 0");
        }
        
        var getShoppingCartResult = await GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var existingEntry = cart.NewTickets.FirstOrDefault(t => t.TicketTypeId == ticketTypeId);

        if (existingEntry is null)
        {
            return Result.Failure(StatusCodes.Status404NotFound, "the shopping cart does not contain a ticket of this type");
        }

        if (existingEntry.Quantity < amount)
        {
            return Result.Failure(StatusCodes.Status400BadRequest,
                $"the shopping cart does not contain {amount} tickets of this type");
        }

        existingEntry.Quantity -= amount;

        if (existingEntry.Quantity == 0)
        {
            cart.NewTickets.Remove(existingEntry);
        }

        var updateShoppingCartResult = await UpdateShoppingCartAsync(customerEmail, cart);

        if (updateShoppingCartResult.IsError)
        {
            return Result.PropagateError(updateShoppingCartResult);
        }
        
        return Result.Success();
    }

    private static string GetCartKey(string customerEmail)
    {
        return $"cart:{customerEmail}";
    }
}