using System.Reflection.Metadata.Ecma335;
using TickAPI.Events.Models;

namespace TickAPI.Organizers.Models;

public class Organizer
{
    public String Id { get; set; }
    public String Email { get; set; }
    public String Login { get; set; }
    public String PasswordHash { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public DateTime CreationDate { get; set; }
    public String OrganizerName { get; set; }
    public bool IsVerified { get; set; }
    public ICollection<Event> Events { get; set; }
}