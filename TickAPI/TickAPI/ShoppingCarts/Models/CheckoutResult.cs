using TickAPI.Common.Payment.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Models;

public record CheckoutResult(
    List<Ticket> BoughtTickets,
    PaymentResponsePG PaymentResponse
);
