using TickAPI.Common.Results;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartService
{
    public Task<Result> AddNewTicketAsync(Guid ticketTypeId, string customerEmail, string? nameOnTicket, string? seats);
    public Task<Result> GetTicketsAsync();
    public Task<Result> RemoveTicketAsync();
    public Task<Result> CheckoutAsync();
}