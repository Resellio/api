using TickAPI.Common.Results;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartService
{
    public Task<Result> AddTicketAsync();
    public Task<Result> GetTicketsAsync();
    public Task<Result> RemoveTicketAsync();
    public Task<Result> CheckoutAsync();
}