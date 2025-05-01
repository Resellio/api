using System.Text.Json.Serialization;

namespace TickAPI.Common.Payment.Models;

public record PaymentRequestPG(
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("card_number")] string CardNumber,
    [property: JsonPropertyName("card_expiry")] string CardExpiry,
    [property: JsonPropertyName("cvv")] string CVV,
    [property: JsonPropertyName("force_error")] bool ForceError
);