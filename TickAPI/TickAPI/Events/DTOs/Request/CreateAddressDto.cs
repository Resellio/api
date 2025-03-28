using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Request;

public record CreateAddressDto(

    string Country,
    string City,
    string? Street,
    uint? HouseNumber,
    uint? FlatNumber,
    string PostalCode);