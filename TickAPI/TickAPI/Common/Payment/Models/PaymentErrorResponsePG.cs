using System.Text.Json.Serialization;

namespace TickAPI.Common.Payment.Models;

public record PaymentErrorResponsePG(
    [property: JsonPropertyName("error")] string Error
);
