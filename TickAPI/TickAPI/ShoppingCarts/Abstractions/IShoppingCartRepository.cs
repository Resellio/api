using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.Models;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartRepository
{
    public Task<Result<ShoppingCart>> GetShoppingCartByEmailAsync(string customerEmail);
    public Task<Result> UpdateShoppingCartAsync(string customerEmail, ShoppingCart shoppingCart);
}