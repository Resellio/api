namespace TickAPI.Customers.DTOs.Response;

public record AboutMeResponseDto(
    string Email,
    string FirstName,
    string LastName,
    DateTime CreationDate
);