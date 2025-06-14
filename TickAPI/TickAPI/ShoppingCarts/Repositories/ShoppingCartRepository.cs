using Microsoft.Extensions.Options;
using TickAPI.Common.Redis.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.Models;
using TickAPI.ShoppingCarts.Options;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Repositories;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly IRedisService _redisService;
    private readonly TimeSpan _defaultExpiry;

    public ShoppingCartRepository(IRedisService redisService, IOptions<ShoppingCartOptions> options)
    {
        _redisService = redisService;
        _defaultExpiry = TimeSpan.FromMinutes(options.Value.LifetimeMinutes);
    }
    
    public async Task<Result<ShoppingCart>> GetShoppingCartByEmailAsync(string customerEmail)
    {
        var cartKey = GetCartKey(customerEmail);
        ShoppingCart? cart;
        
        try
        {
            cart = await _redisService.GetObjectAsync<ShoppingCart>(cartKey);
            await _redisService.KeyExpireAsync(cartKey, _defaultExpiry);
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
            var res = await _redisService.SetObjectAsync(cartKey, shoppingCart, _defaultExpiry);
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
        if (amount == 0)
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
        
        var incrementTicketAmountResult = await IncrementAmountOfTicketTypeAsync(ticketTypeId, amount);

        if (incrementTicketAmountResult.IsError)
        {
            return Result.PropagateError(incrementTicketAmountResult);
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
        if (amount == 0)
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
        
        var decrementTicketAmountResult = await DecrementAmountOfTicketTypeAsync(ticketTypeId, amount);

        if (decrementTicketAmountResult.IsError)
        {
            return Result.PropagateError(decrementTicketAmountResult);
        }

        var updateShoppingCartResult = await UpdateShoppingCartAsync(customerEmail, cart);

        if (updateShoppingCartResult.IsError)
        {
            return Result.PropagateError(updateShoppingCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result<long>> GetAmountOfTicketTypeAsync(Guid ticketTypeId)
    {
        long? amount; 
        
        try
        {
            amount = await _redisService.GetLongValueAsync(GetAmountKey(ticketTypeId));
        }
        catch (Exception e)
        {
            return Result<long>.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }
        
        if (amount is null)
        {
            return Result<long>.Success(0);
        }

        return Result<long>.Success(amount.Value);
    }

    public async Task<Result> SetAmountOfTicketTypeAsync(Guid ticketTypeId, long amount)
    {
        bool success;
        
        try
        {
            success = await _redisService.SetLongValueAsync(GetAmountKey(ticketTypeId), amount);
        }
        catch (Exception e)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        if (!success)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "the amount of tickets could not be updated");
        }
        
        return Result.Success();
    }

    public async Task<Result<long>> IncrementAmountOfTicketTypeAsync(Guid ticketTypeId, long amount)
    {
        long? newAmount; 
        
        try
        {
            newAmount = await _redisService.IncrementValueAsync(GetAmountKey(ticketTypeId), amount);
        }
        catch (Exception e)
        {
            return Result<long>.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        return Result<long>.Success(newAmount.Value);
    }

    public async Task<Result<long>> DecrementAmountOfTicketTypeAsync(Guid ticketTypeId, long amount)
    {
        long? newAmount; 
        
        try
        {
            newAmount = await _redisService.DecrementValueAsync(GetAmountKey(ticketTypeId), amount);
        }
        catch (Exception e)
        {
            return Result<long>.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        return Result<long>.Success(newAmount.Value);
    }

    public async Task<Result> RemoveAmountOfTicketTypeAsync(Guid ticketTypeId)
    {
        bool success;
        
        try
        {
            success = await _redisService.DeleteKeyAsync(GetAmountKey(ticketTypeId));
        }
        catch (Exception e)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        if (!success)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "the amount of tickets could not be updated");
        }
        
        return Result.Success();
    }

    public async Task<Result> AddResellTicketToCartAsync(string customerEmail, Guid ticketId)
    {
        var getShoppingCartResult = await GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        cart.ResellTickets.Add(new ShoppingCartResellTicket {TicketId = ticketId});
        
        var setKeyResult = await SetTicketKeyAsync(ticketId);

        if (setKeyResult.IsError)
        {
            return Result.PropagateError(setKeyResult);
        }
        
        var updateShoppingCartResult = await UpdateShoppingCartAsync(customerEmail, cart);

        if (updateShoppingCartResult.IsError)
        {
            return Result.PropagateError(updateShoppingCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result<bool>> CheckResellTicketAvailabilityAsync(Guid ticketId)
    {
        bool exists;
        
        try
        {
            exists = await _redisService.KeyExistsAsync(GetResellTicketKey(ticketId));
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }
        
        return Result<bool>.Success(!exists);
    }

    public async Task<Result> RemoveResellTicketFromCartAsync(string customerEmail, Guid ticketId)
    {
        var getShoppingCartResult = await GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var existingEntry = cart.ResellTickets.FirstOrDefault(t => t.TicketId == ticketId);

        if (existingEntry is null)
        {
            return Result.Failure(StatusCodes.Status404NotFound, "the shopping cart does not contain this ticket");
        }
        
        cart.ResellTickets.Remove(existingEntry);
        
        var deleteKeyResult = await DeleteTicketKeyAsync(ticketId);

        if (deleteKeyResult.IsError)
        {
            return Result.PropagateError(deleteKeyResult);
        }

        var updateShoppingCartResult = await UpdateShoppingCartAsync(customerEmail, cart);

        if (updateShoppingCartResult.IsError)
        {
            return Result.PropagateError(updateShoppingCartResult);
        }
        
        return Result.Success();
    }

    private async Task<Result> SetTicketKeyAsync(Guid ticketId)
    {
        bool success;
        
        try
        {
            success = await _redisService.SetStringAsync(GetResellTicketKey(ticketId), string.Empty, _defaultExpiry);
        }
        catch (Exception e)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        if (!success)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "the ticket key could not be updated");
        }
        
        return Result.Success();
    }
    
    private async Task<Result> DeleteTicketKeyAsync(Guid ticketId)
    {
        bool success;
        
        try
        {
            success = await _redisService.DeleteKeyAsync(GetResellTicketKey(ticketId));
        }
        catch (Exception e)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }

        if (!success)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, "the ticket key could not be deleted");
        }
        
        return Result.Success();
    }

    private static string GetCartKey(string customerEmail)
    {
        return $"cart:{customerEmail}";
    }

    private static string GetAmountKey(Guid ticketTypeId)
    {
        return $"amount:{ticketTypeId}";
    }

    private static string GetResellTicketKey(Guid ticketId)
    {
        return $"resell:{ticketId}";
    }
}