using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;
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

        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        
        ticketRepositoryMock
            .Setup(m => m.GetAllTicketsByTicketType(type))
            .Returns(ticketList.AsQueryable());

        var sut = new TicketService(ticketRepositoryMock.Object, paginationServiceMock.Object);

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

        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        
        ticketRepositoryMock
            .Setup(m => m.GetAllTicketsByTicketType(type))
            .Returns(ticketList.AsQueryable());

        var sut = new TicketService(ticketRepositoryMock.Object, paginationServiceMock.Object);

        // Act
        var result = sut.GetNumberOfAvailableTicketsByType(type);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("The number of available tickets is negative.", result.ErrorMsg);
    }
    
     [Fact]
    public async Task GetTicketsForResellAsync_WhenDataIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        int pageSize = 10;
        int page = 0;
        
        var ticket1 = new Ticket
        {
            Id = Guid.NewGuid(),
            ForResell = true,
            Type = new TicketType
            {
                Price = 50m,
                Currency = "USD",
                Description = "VIP"
            },
            Seats = "A1"
        };
        
        var ticket2 = new Ticket
        {
            Id = Guid.NewGuid(),
            ForResell = true,
            Type = new TicketType
            {
                Price = 30m,
                Currency = "USD",
                Description = "Standard"
            },
            Seats = "B2"
        };
        
        var allTickets = new List<Ticket> { ticket1, ticket2 }.AsQueryable();
        
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(repo => repo.GetTicketsByEventId(eventId))
            .Returns(allTickets);
        
        var paginatedTickets = new PaginatedData<Ticket>(
            new List<Ticket> { ticket1, ticket2 },
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 2)
        );
        
        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Ticket>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Ticket>>.Success(paginatedTickets));
        
        var mappedDto1 = new GetTicketForResellResponseDto(
            ticket1.Id, 
            ticket1.Type.Price, 
            ticket1.Type.Currency, 
            ticket1.Type.Description, 
            ticket1.Seats
        );
        
        var mappedDto2 = new GetTicketForResellResponseDto(
            ticket2.Id, 
            ticket2.Type.Price, 
            ticket2.Type.Currency, 
            ticket2.Type.Description, 
            ticket2.Seats
        );
        
        var mappedData = new PaginatedData<GetTicketForResellResponseDto>(
            new List<GetTicketForResellResponseDto> { mappedDto1, mappedDto2 },
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 2)
        );
        
        paginationServiceMock.Setup(p => p.MapData(
                paginatedTickets,
                It.IsAny<Func<Ticket, GetTicketForResellResponseDto>>()))
            .Returns(mappedData);
        
        var sut = new TicketService(ticketRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var result = await sut.GetTicketsForResellAsync(eventId, page, pageSize);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Data.Count);
        Assert.Equal(mappedDto1, result.Value!.Data[0]);
        Assert.Equal(mappedDto2, result.Value!.Data[1]);
    }
    
    [Fact]
    public async Task GetTicketsForResellAsync_WhenNoTicketsForResell_ShouldReturnEmptyList()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        int pageSize = 10;
        int page = 0;
        
        var tickets = new List<Ticket>
        {
            new Ticket
            {
                Id = Guid.NewGuid(),
                ForResell = false,
                Type = new TicketType
                {
                    Price = 50m,
                    Currency = "USD",
                    Description = "VIP"
                }
            },
            new Ticket
            {
                Id = Guid.NewGuid(),
                ForResell = false,
                Type = new TicketType
                {
                    Price = 30m,
                    Currency = "USD",
                    Description = "Standard"
                }
            }
        }.AsQueryable();
        
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(repo => repo.GetTicketsByEventId(eventId))
            .Returns(tickets);
        
        var paginatedData = new PaginatedData<Ticket>(
            new List<Ticket>(),
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 0)
        );
        
        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Ticket>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Ticket>>.Success(paginatedData));
        
        var mappedData = new PaginatedData<GetTicketForResellResponseDto>(
            new List<GetTicketForResellResponseDto>(),
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 0)
        );
        
        paginationServiceMock.Setup(p => p.MapData(
                paginatedData,
                It.IsAny<Func<Ticket, GetTicketForResellResponseDto>>()))
            .Returns(mappedData);
        
        var sut = new TicketService(ticketRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var result = await sut.GetTicketsForResellAsync(eventId, page, pageSize);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Data);
    }
    
    [Fact]
    public async Task GetTicketsForResellAsync_WhenPaginationFails_ShouldPropagateError()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        int pageSize = 10;
        int page = 0;
        const string errorMsg = "Invalid pagination parameters";
        const int statusCode = 400;
        
        var tickets = new List<Ticket>
        {
            new Ticket
            {
                Id = Guid.NewGuid(),
                ForResell = true,
                Type = new TicketType
                {
                    Price = 50m,
                    Currency = "USD",
                    Description = "VIP"
                }
            }
        }.AsQueryable();
        
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(repo => repo.GetTicketsByEventId(eventId))
            .Returns(tickets);
            
        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Ticket>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Ticket>>.Failure(statusCode, errorMsg));
        
        var sut = new TicketService(ticketRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var result = await sut.GetTicketsForResellAsync(eventId, page, pageSize);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }
    
    [Fact]
    public async Task GetTicketsForResellAsync_WhenNoTicketsForEvent_ShouldReturnEmptyList()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        int pageSize = 10;
        int page = 0;
        
        var tickets = new List<Ticket>().AsQueryable();
        
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(repo => repo.GetTicketsByEventId(eventId))
            .Returns(tickets);
        
        var paginatedData = new PaginatedData<Ticket>(
            new List<Ticket>(),
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 0)
        );
        
        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Ticket>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Ticket>>.Success(paginatedData));
        
        var mappedData = new PaginatedData<GetTicketForResellResponseDto>(
            new List<GetTicketForResellResponseDto>(),
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 0)
        );
        
        paginationServiceMock.Setup(p => p.MapData(
                paginatedData,
                It.IsAny<Func<Ticket, GetTicketForResellResponseDto>>()))
            .Returns(mappedData);
        
        var sut = new TicketService(ticketRepositoryMock.Object, paginationServiceMock.Object);
        
        // Act
        var result = await sut.GetTicketsForResellAsync(eventId, page, pageSize);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Data);
    }
}