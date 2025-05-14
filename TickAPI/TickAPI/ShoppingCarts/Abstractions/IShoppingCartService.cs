using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.DTOs.Response;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartService
{
    public Task<Result> AddNewTicketAsync(Guid ticketTypeId, string customerEmail, string? nameOnTicket, string? seats);
    public Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsAsync(string customerEmail);
    public Task<Result> RemoveTicketAsync();
    public Task<Result> CheckoutAsync();
}