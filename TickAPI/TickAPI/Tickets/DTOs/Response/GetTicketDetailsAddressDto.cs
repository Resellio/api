namespace TickAPI.Tickets.DTOs.Response;

public record GetTicketDetailsAddressDto(
    string Country,
    string City,
    string PostalCode,
    string? Street,
    uint? HouseNumber,
    uint? FlatNumber
);