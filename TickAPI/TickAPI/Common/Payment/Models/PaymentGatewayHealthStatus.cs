using System.Text.Json.Serialization;

namespace TickAPI.Common.Payment.Models;

public record PaymentGatewayHealthStatus(
    [property: JsonPropertyName("status")] string Status
);
