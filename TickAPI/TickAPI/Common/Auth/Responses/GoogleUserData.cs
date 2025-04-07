using System.Text.Json.Serialization;

namespace TickAPI.Common.Auth.Responses;

public record GoogleUserData(
    [property :JsonPropertyName("email")] string Email,
    [property :JsonPropertyName("given_name")] string GivenName,
    [property :JsonPropertyName("family_name")] string FamilyName
);