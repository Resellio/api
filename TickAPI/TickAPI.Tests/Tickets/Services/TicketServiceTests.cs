using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Addresses.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Models;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Tickets.Models;
using TickAPI.Tickets.Services;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Tickets.Services;

public class TicketServiceTests
{
    [Fact]
    public void GetNumberOfAvailableTicketsByType_AmountsAreCorrect_ShouldReturnCorrectNumberOfTickets()
    {
        // Arrange
        var type = new TicketType { MaxCount = 30 };
        var ticketList = new List<Ticket>(new Ticket[10]);

        Mock<ITicketRepository> ticketRepositoryMock = new Mock<ITicketRepository>();
        
        ticketRepositoryMock
            .Setup(m => m.GetAllTicketsByTicketType(type))
            .Returns(ticketList.AsQueryable());

        var sut = new TicketService(ticketRepositoryMock.Object);

        // Act
        var result = sut.GetNumberOfAvailableTicketsByType(type);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20u, result.Value);
    }

    [Fact]
    public void GetNumberOfAvailableTicketsByType_WhenMoreTicketExistThanMaxCount_ShouldReturnError()
    {
        // Arrange
        var type = new TicketType { MaxCount = 30 };
        var ticketList = new List<Ticket>(new Ticket[50]);

        Mock<ITicketRepository> ticketRepositoryMock = new Mock<ITicketRepository>();
        
        ticketRepositoryMock
            .Setup(m => m.GetAllTicketsByTicketType(type))
            .Returns(ticketList.AsQueryable());

        var sut = new TicketService(ticketRepositoryMock.Object);

        // Act
        var result = sut.GetNumberOfAvailableTicketsByType(type);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("The number of available tickets is negative.", result.ErrorMsg);
    }

    [Fact]
    public async Task GetTicketDetailsAsync_WhenTicketExistsForTheUser_ShouldReturnTicketDetails()
    {
        
        // Arrange
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ForResell = false,
            NameOnTicket = "NameOnTicket",
            Seats = null,
            Type = new TicketType
            {
                Currency = "USD",
                Price = 20,
                Event = new Event
                {
                    Name = "EventName",
                    StartDate = new DateTime(2025, 10, 10),
                    EndDate = new DateTime(2025, 10, 20),
                    Organizer = new Organizer
                    {
                        DisplayName = "organizerName",
                    },
                    Address = new Address
                    {
                        City = "Warsaw",
                        Country = "Poland",
                        PostalCode = "12345",
                        FlatNumber = null,
                        HouseNumber = null,
                        Street = "Street",
                    }
                }
            },
        };
        string email = "123@123.com";
        
        Mock<ITicketRepository> ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(m =>
            m.CheckIfTicketBelongsToCustomerAsync(ticket.Id, email)).ReturnsAsync(Result<bool>.Success(true));

        ticketRepositoryMock.Setup(m => m.GetTicketByIdAsync(ticket.Id)).ReturnsAsync(Result<Ticket>.Success(ticket));
        
        var sut = new TicketService(ticketRepositoryMock.Object);
        
        // Act

        var res = await sut.GetTicketDetailsAsync(email, ticket.Id);
        
        // Assert

        Assert.True(res.IsSuccess);
        var details = res.Value;
        Assert.NotNull(details);
        
        Assert.Equal(ticket.NameOnTicket, details.NameOnTicket);
        Assert.Equal(ticket.Seats, details.Seats);
        Assert.Equal(ticket.Type.Currency, details.Currency);
        Assert.Equal(ticket.Type.Price, details.Price);
        Assert.Equal(ticket.Type.Event.StartDate, details.StartDate);
        Assert.Equal(ticket.Type.Event.EndDate, details.EndDate);
        Assert.Equal(ticket.Type.Event.Organizer.DisplayName, details.OrganizerName);
        Assert.Equal(ticket.Type.Event.Address.Street, details.Address.Street);
        Assert.Equal(ticket.Type.Event.Address.HouseNumber, details.Address.HouseNumber);
        Assert.Equal(ticket.Type.Event.Address.FlatNumber, details.Address.FlatNumber);
        Assert.Equal(ticket.Type.Event.Address.PostalCode, details.Address.PostalCode);
        Assert.Equal(ticket.Type.Event.Address.City, details.Address.City);
        Assert.Equal(ticket.Type.Event.Address.Country, details.Address.Country);
        
    }

    [Fact]
    public async Task GetTicketDetailsAsync_WhenTicketDoesntExistsForTheUser_ShouldReturnError()
    {
        
        // Arrange
        
        Guid  ticketId = Guid.NewGuid();
        string email = "123@123.com";
        
        Mock<ITicketRepository> ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(m => m.CheckIfTicketBelongsToCustomerAsync(ticketId, email)).
            ReturnsAsync(Result<bool>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist " +
                                                                             "for this user"));
        
        var sut = new TicketService(ticketRepositoryMock.Object);
        
        // Act

        var res = await sut.GetTicketDetailsAsync(email, ticketId);
        
        // Assert
        
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, res.StatusCode);
        Assert.Equal("Ticket with this id doesn't exist for this user", res.ErrorMsg);
    }
}