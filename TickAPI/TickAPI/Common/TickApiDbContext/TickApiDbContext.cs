using Microsoft.EntityFrameworkCore;
using TickAPI.Admins.Models;
using TickAPI.Categories.Models;
using TickAPI.Customers.Models;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Common.TickApiDbContext;

public class TickApiDbContext : DbContext
{
    public TickApiDbContext(DbContextOptions<TickApiDbContext> options) : base(options)
    { }
    
    
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketType> TicketTypes { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Category> Categories { get; set; }
}