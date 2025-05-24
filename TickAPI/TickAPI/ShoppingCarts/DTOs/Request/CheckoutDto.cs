namespace TickAPI.ShoppingCarts.DTOs.Request;

public record CheckoutDto(
    decimal Amount,
    string Currency,
    string CardNumber,
    string CardExpiry,
    string Cvv
);