using System.Text.Json.Serialization;

namespace TickAPI.Common.Payment.Models;

public record PaymentResponsePG(
    [property: JsonPropertyName("transaction_id")] string TransactionId,
    [property: JsonPropertyName("status")] string Status
);
