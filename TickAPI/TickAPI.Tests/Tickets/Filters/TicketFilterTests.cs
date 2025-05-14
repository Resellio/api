using TickAPI.Customers.Models;
using TickAPI.Events.Models;
using TickAPI.Tickets.Filters;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Tickets.Filters;

public class TicketFilterTests
{
     private static List<Ticket> GetTestTickets() =>
    [
        new Ticket
        {
            Id = Guid.NewGuid(),
            Type = new TicketType 
            { 
                Price = 100,
                Event = new Event 
                { 
                    Name = "Concert A",
                    Description = "An amazing rock concert"
                }
            },
            Owner = new Customer { FirstName = "John", LastName = "Doe" },
            NameOnTicket = "John Doe",
            Seats = "A12",
            ForResell = true,
            Used = false
        },

        new Ticket
        {
            Id = Guid.NewGuid(),
            Type = new TicketType 
            { 
                Price = 50,
                Event = new Event 
                { 
                    Name = "Concert B",
                    Description = "Chill jazz night"
                }
            },
            Owner = new Customer { FirstName = "Jane", LastName = "Smith" },
            NameOnTicket = "Jane Smith",
            Seats = "B5",
            ForResell = false,
            Used = true
        },

        new Ticket
        {
            Id = Guid.NewGuid(),
            Type = new TicketType 
            { 
                Price = 200,
                Event = new Event 
                { 
                    Name = "Conference",
                    Description = "Tech event for developers"
                }
            },
            Owner = new Customer { FirstName = "Mike", LastName = "Johnson" },
            NameOnTicket = "Mike Johnson",
            Seats = "C8",
            ForResell = false,
            Used = false
        }
    ];

    [Fact]
    public void FilterUsedTickets_ShouldReturnOnlyUsedTickets()
    {
        // Arrange
        var tickets = GetTestTickets();
        var sut = new TicketFilter(tickets.AsQueryable());
        
        //Act
        sut.FilterUsedTickets();
        var result = sut.GetTickets().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(tickets[1], result);
    }
    
    [Fact]
    public void FilterUnusedTickets_ShouldReturnOnlyUnusedTickets()
    {
        // Arrange
        var tickets = GetTestTickets();
        var sut = new TicketFilter(tickets.AsQueryable());
        
        // Act
        sut.FilterUnusedTickets();
        var result = sut.GetTickets().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(tickets[0], result);
        Assert.Contains(tickets[2], result);
    }
    
    [Fact]
    public void FilterTicketsForResell_ShouldReturnOnlyTicketsForResell()
    {
        // Arrange
        var tickets = GetTestTickets();
        var sut = new TicketFilter(tickets.AsQueryable());
        
        //Act
        sut.FilterTicketsForResell();
        var result = sut.GetTickets().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(tickets[0], result);
    }
    
    [Fact]
    public void FilterTicketsNotForResell_ShouldReturnOnlyTicketsNotForResell()
    {
        // Arrange
        var tickets = GetTestTickets();
        var sut = new TicketFilter(tickets.AsQueryable());
        
        // Act
        sut.FilterTicketsNotForResell();
        var result = sut.GetTickets().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(tickets[1], result);
        Assert.Contains(tickets[2], result);
    }
    
    [Fact]
    public void FilterTicketsByEventName_ShouldReturnTicketsWithMatchingEventName()
    {
        // Arrange
        var tickets = GetTestTickets();

        // Act
        var ticketFilter = new TicketFilter(tickets.AsQueryable());
        ticketFilter.FilterTicketsByEventName("concert");
        var result = ticketFilter.GetTickets().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(tickets[0], result);
        Assert.Contains(tickets[1], result);
    }
    
    [Fact]
    public void FilterTicketsByEventName_CaseInsensitive_ShouldReturnMatchingTickets()
    {
        // Arrange
        var tickets = GetTestTickets();

        // Act
        var ticketFilter = new TicketFilter(tickets.AsQueryable());
        ticketFilter.FilterTicketsByEventName("cONcErt a");
        var result = ticketFilter.GetTickets().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(tickets[0], result);
    }
    
    [Fact]
    public void FilterTicketsByEventName_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var tickets = GetTestTickets();

        // Act
        var ticketFilter = new TicketFilter(tickets.AsQueryable());
        ticketFilter.FilterTicketsByEventName("nonexistent event");
        var result = ticketFilter.GetTickets().ToList();

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void GetTickets_WithNoFilters_ShouldReturnAllTickets()
    {
        // Arrange
        var tickets = GetTestTickets();

        // Act
        var ticketFilter = new TicketFilter(tickets.AsQueryable());
        var result = ticketFilter.GetTickets().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(tickets[0], result);
        Assert.Contains(tickets[1], result);
        Assert.Contains(tickets[2], result);
    }
}