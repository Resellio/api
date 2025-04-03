using TickAPI.Tickets.Models;

namespace TickAPI.Customers.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreationDate { get; set; }
    public ICollection<Ticket> Tickets { get; set; }    
}