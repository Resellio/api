namespace TickAPI.Addresses.DTOs.Request;

public record CreateAddressDto(
    string Country,
    string City,
    string? Street,
    uint? HouseNumber,
    uint? FlatNumber,
    string PostalCode
);