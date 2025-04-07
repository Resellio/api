namespace TickAPI.Customers.DTOs.Response;

public record AboutMeCustomerResponseDto(
    string Email,
    string FirstName,
    string LastName,
    DateTime CreationDate
);