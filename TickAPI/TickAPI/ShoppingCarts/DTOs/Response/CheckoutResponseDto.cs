namespace TickAPI.ShoppingCarts.DTOs.Response;

public record CheckoutResponseDto(
    string TransactionId,
    string Status
);