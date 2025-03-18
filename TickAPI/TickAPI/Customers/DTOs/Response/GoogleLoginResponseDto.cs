namespace TickAPI.Customers.DTOs.Response;

public record GoogleLoginResponseDto(
    string Token,
    bool IsNewCustomer
);