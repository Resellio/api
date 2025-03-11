namespace TickAPI.Events.Models;

public class Address
{
    public Guid Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string? Street { get; set; }
    public uint? HouseNumber { get; set; }
    public uint? FlatNumber { get; set; }
    public string PostalCode { get; set; }
}