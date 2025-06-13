using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.Models;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartRepository
{
    public Task<Result<ShoppingCart>> GetShoppingCartByEmailAsync(string customerEmail);
    public Task<Result> UpdateShoppingCartAsync(string customerEmail, ShoppingCart shoppingCart);
    public Task<Result> AddNewTicketsToCartAsync(string customerEmail, Guid ticketTypeId, uint amount);
    public Task<Result> RemoveNewTicketsFromCartAsync(string customerEmail, Guid ticketTypeId, uint amount);
    public Task<Result<long>> GetAmountOfTicketTypeAsync(Guid ticketTypeId);
    public Task<Result> SetAmountOfTicketTypeAsync(Guid ticketTypeId, long amount);
    public Task<Result<long>> IncrementAmountOfTicketTypeAsync(Guid ticketTypeId, long amount);
    public Task<Result<long>> DecrementAmountOfTicketTypeAsync(Guid ticketTypeId, long amount);
    public Task<Result> RemoveAmountOfTicketTypeAsync(Guid ticketTypeId);
    public Task<Result> AddResellTicketToCartAsync(string customerEmail, Guid ticketId);
    public Task<Result<bool>> CheckResellTicketAvailabilityAsync(Guid ticketId);
}