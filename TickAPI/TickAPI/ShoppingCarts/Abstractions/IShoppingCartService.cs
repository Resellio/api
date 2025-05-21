using TickAPI.Common.Payment.Models;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.ShoppingCarts.DTOs.Response;

namespace TickAPI.ShoppingCarts.Abstractions;

public interface IShoppingCartService
{
    public Task<Result> AddNewTicketsToCartAsync(Guid ticketTypeId, uint amount, string customerEmail);
    public Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsFromCartAsync(string customerEmail);
    public Task<Result> RemoveNewTicketsFromCartAsync(Guid ticketTypeId, uint amount, string customerEmail);
    public Task<Result<Dictionary<string, decimal>>> GetDueAmountAsync(string customerEmail);
    public Task<Result<PaymentResponsePG>> CheckoutAsync(string customerEmail, decimal amount, string currency,
        string cardNumber,
        string cardExpiry, string cvv);
}