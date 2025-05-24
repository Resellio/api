namespace TickAPI.ShoppingCarts.DTOs.Request;

public record RemoveNewTicketDto(
    Guid TicketTypeId,
    uint Amount
);