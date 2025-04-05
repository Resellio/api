namespace TickAPI.Events.DTOs.Response;

public record GetEventResponseAddressDto(
    string Country,
    string City,
    string PostalCode,
    string? Street,
    uint? HouseNumber,
    uint? FlatNumber
);
