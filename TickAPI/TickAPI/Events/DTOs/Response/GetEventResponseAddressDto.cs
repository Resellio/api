namespace TickAPI.Events.DTOs.Response;

public record GetEventResponseAddressDto(
    string Country,
    string City,
    string PostalCode,
    string? Stree,
    uint? HouseNumber,
    uint? FlatNumber
);
