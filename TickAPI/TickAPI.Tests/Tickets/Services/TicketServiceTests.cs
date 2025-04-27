using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Results.Generic;
using TickAPI.Tickets.Abstractions;
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
}