using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.DTOs.Response;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartService
{
    public Task<Result> AddNewTicketsToCartAsync(Guid ticketTypeId, uint amount, string customerEmail);
    public Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsFromCartAsync(string customerEmail);
    public Task<Result> RemoveNewTicketsFromCartAsync(Guid ticketTypeId, uint amount, string customerEmail);
    public Task<Result> CheckoutAsync();
}