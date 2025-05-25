namespace TickAPI.ShoppingCarts.DTOs.Request;

public record AddNewTicketDto(
    Guid TicketTypeId,
    uint Amount
);