using Microsoft.EntityFrameworkCore;
using TickAPI.Addresses.Models;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = Guid.Parse("ec3daf69-baa9-4fcd-a674-c09884a57272"),
                Name = "Music"
            },
            new Category
            {
                Id = Guid.Parse("de89dd76-3b29-43e1-8f4b-5278b1b8bde2"),
                Name = "Sports"
            },
            new Category
            {
                Id = Guid.Parse("ea58370b-2a17-4770-abea-66399ad69fb8"),
                Name = "Conferences"
            },
            new Category
            {
                Id = Guid.Parse("4a086d9e-59de-4fd1-a1b2-bd9b5eec797c"),
                Name = "Theatre"
            },
            new Category
            {
                Id = Guid.Parse("5f8dbe65-30be-453f-8f22-191a11b2977b"),
                Name = "Comedy"
            },
            new Category
            {
                Id = Guid.Parse("4421327a-4bc8-4706-bec0-666f78ed0c69"),
                Name = "Workshops"
            }
        );
    }
}