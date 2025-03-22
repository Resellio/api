namespace TickAPI.Events.Models;
using TickAPI.Events.DTOs.Request;
public class Address
{
    public Guid Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string? Street { get; set; }
    public uint? HouseNumber { get; set; }
    public uint? FlatNumber { get; set; }
    public string PostalCode { get; set; }


    public static Address FromDto(AddressDto dto)
    {
        return new Address
        {
            City = dto.City,
            HouseNumber = dto.HouseNumber,
            FlatNumber = dto.FlatNumber,
            PostalCode = dto.PostalCode,
            Street = dto.Street,
            Country = dto.Country
        };
    }
}