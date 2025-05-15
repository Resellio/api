using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.DTOs.Response;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartService
{
    public Task<Result> AddNewTicketToCartAsync(Guid ticketTypeId, string customerEmail, string? nameOnTicket, string? seats);
    public Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsFromCartAsync(string customerEmail);
    public Task<Result> RemoveTicketFromCartAsync();
    public Task<Result> CheckoutAsync();
}