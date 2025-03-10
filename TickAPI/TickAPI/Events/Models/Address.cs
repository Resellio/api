namespace TickAPI.Events.Models;

public class Address
{
    public Guid Id { get; set; }
    public String Country { get; set; }
    public String City { get; set; }
    public String Street { get; set; }
    public uint HouseNumber { get; set; }
    public uint FlatNumber { get; set; }
    public String PostalCode { get; set; }
}