using TickAPI.Common.Redis.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.Models;

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
                return Result.Failure(StatusCodes.Status500InternalServerError, "The shopping cart could not be updated.");
            }
        }
        catch (Exception e)
        {
            return Result.Failure(StatusCodes.Status500InternalServerError, e.Message);
        }
        
        return Result.Success();
    }

    private static string GetCartKey(string customerEmail)
    {
        return $"cart:{customerEmail}";
    }
}