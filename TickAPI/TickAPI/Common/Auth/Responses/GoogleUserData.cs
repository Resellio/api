namespace TickAPI.Common.Auth.Responses;

public record GoogleUserData(
    string Email,
    string FirstName,
    string LastName
);