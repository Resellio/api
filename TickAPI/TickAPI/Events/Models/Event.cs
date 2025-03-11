using TickAPI.Organizers.Models;
using TickAPI.Categories.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Events.Models;

public class Event
{
    public Guid Id { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public uint MinimumAge { get; set; }
    public Organizer Organizer { get; set; }
    public ICollection<Category> Categories { get; set; }
    public ICollection<TicketType> TicketTypes { get; set; }
    public EventStatus EventStatus { get; set; }
    public Address Address { get; set; }
}

public enum EventStatus
{
    TicketsAvailable,
    SoldOut,
    InProgress,
    Finished
}