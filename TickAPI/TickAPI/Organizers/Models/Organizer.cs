﻿using System.Reflection.Metadata.Ecma335;
using TickAPI.Events.Models;

namespace TickAPI.Organizers.Models;

public class Organizer
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Login { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreationDate { get; set; }
    public string OrganizerName { get; set; }
    public bool IsVerified { get; set; }
    public ICollection<Event> Events { get; set; }
}