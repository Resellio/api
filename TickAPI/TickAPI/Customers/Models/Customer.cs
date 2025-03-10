using TickAPI.Tickets.Models;

namespace TickAPI.Customers.Models;

public class Customer
{
    public Guid Id { get; set; }
    public String Email { get; set; }
    public String Login { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public DateTime CreationDate { get; set; }
    public ICollection<Ticket> Tickets { get; set; }    
}