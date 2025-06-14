﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Addresses.Models;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.QR.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Models;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Response;
using TickAPI.Tickets.Models;
using TickAPI.Tickets.Services;
using TickAPI.TicketTypes.Abstractions;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Tickets.Services;

public class TicketServiceTests
{
    [Fact]
    public async Task GetNumberOfAvailableTicketsByType_AmountsAreCorrect_ShouldReturnCorrectNumberOfTickets()
    {
        // Arrange
        var type = new TicketType { MaxCount = 30 };
        var ticketList = new List<Ticket>(new Ticket[10]);

        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        
        ticketRepositoryMock
            .Setup(m => m.GetAllTicketsByTicketType(type))
            .Returns(ticketList.AsQueryable());

        shoppingCartRepositoryMock
            .Setup(s => s.GetAmountOfTicketTypeAsync(type.Id))
            .ReturnsAsync(Result<long>.Success(0));

        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);

        // Act
        var result = await sut.GetNumberOfAvailableTicketsByTypeAsync(type);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20u, result.Value);
    }

    [Fact]
    public async Task GetNumberOfAvailableTicketsByType_WhenMoreTicketExistThanMaxCount_ShouldReturnError()
    {
        // Arrange
        var type = new TicketType { MaxCount = 30 };
        var ticketList = new List<Ticket>(new Ticket[50]);

        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        
        ticketRepositoryMock
            .Setup(m => m.GetAllTicketsByTicketType(type))
            .Returns(ticketList.AsQueryable());
        
        shoppingCartRepositoryMock
            .Setup(s => s.GetAmountOfTicketTypeAsync(type.Id))
            .ReturnsAsync(Result<long>.Success(0));

        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);

        // Act
        var result = await sut.GetNumberOfAvailableTicketsByTypeAsync(type);

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
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        
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
        
        var qrServiceMock = new Mock<IQRCodeService>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
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
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        
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
        var qrServiceMock = new Mock<IQRCodeService>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
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
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
            
        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Ticket>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Ticket>>.Failure(statusCode, errorMsg));
        
        var qrServiceMock = new Mock<IQRCodeService>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
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
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        
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
        var qrServiceMock = new Mock<IQRCodeService>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
        // Act
        var result = await sut.GetTicketsForResellAsync(eventId, page, pageSize);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Data);
    }

    [Fact]
    public async Task GetTicketDetailsAsync_WhenTicketExistsForTheUser_ShouldReturnTicketDetails()
    {
        
        // Arrange
        var eventGuid = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ForResell = false,
            NameOnTicket = "NameOnTicket",
            Seats = null,
            Type = new TicketType
            {
                Id = eventGuid,
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
        const string email = "123@123.com";
        const string scanurl = "http://localhost";
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        
        var paginationServiceMock = new Mock<IPaginationService>();

        ticketRepositoryMock.Setup(m => m.GetTicketWithDetailsByIdAndEmailAsync(ticket.Id, email))
            .ReturnsAsync(Result<Ticket>.Success(ticket));
        
        var qrServiceMock = new Mock<IQRCodeService>();
        qrServiceMock.Setup(m => m.GenerateQrCode(scanurl)).Returns([]);
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
        // Act

        var res = await sut.GetTicketDetailsAsync(ticket.Id, email, scanurl);
        
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
    public async Task GetTicketDetailsAsync_WhenTicketDoesNotExistForTheUser_ShouldReturnError()
    {
        
        // Arrange
        
        var  ticketId = Guid.NewGuid();
        const string email = "123@123.com";
        const string scanUrl = "http://localhost";
        
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(m => m.GetTicketWithDetailsByIdAndEmailAsync(ticketId, email)).
            ReturnsAsync(Result<Ticket>.Failure(StatusCodes.Status404NotFound, "Ticket with this id doesn't exist " +
                                                                             "for this user"));
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
        // Act

        var res = await sut.GetTicketDetailsAsync(ticketId, email, scanUrl);
        
        // Assert
        
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, res.StatusCode);
        Assert.Equal("Ticket with this id doesn't exist for this user", res.ErrorMsg);
    }
    
    [Fact]
    public async Task GetTicketsForCustomerAsync_WithValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var email = "test@example.com";
        var page = 0;
        var pageSize = 10;
        
        var tickets = new List<Ticket>
        {
            new Ticket
            {
                Id = new Guid(),
                Used = false,
                Type = new TicketType
                {
                    Event = new Event
                    {
                        Name = "EventName",
                        StartDate = new DateTime(2025, 10, 10),
                        EndDate = new DateTime(2025, 10, 20),
                    }
                }
            },
            new Ticket
            {
                Id = new Guid(),
                Used = false,
                Type = new TicketType
                {
                    Event = new Event
                    {
                        Name = "EventName2",
                        StartDate = new DateTime(2025, 11, 10),
                        EndDate = new DateTime(2025, 11, 20),
                    }
                }
            }
        };
        
        var paginatedData = new PaginatedData<Ticket>
        (
            tickets,
            page, 
            pageSize,
            false,
            false,
            new PaginationDetails(0, 2)
        );
        var mappedData1 = new GetTicketForCustomerDto(tickets[0].Id, "EventName", new DateTime(2025, 10, 10), new DateTime(2025, 10, 20), false, false, null, null);
        var mappedData2 = new GetTicketForCustomerDto(tickets[1].Id, "EventName2", new DateTime(2025, 11, 10), new DateTime(2025, 11, 20), false, false, null, null);
        var mappedPaginatedData = new PaginatedData<GetTicketForCustomerDto>
        (
            new List<GetTicketForCustomerDto>{mappedData1, mappedData2},
            page,
            pageSize,
            false,
            false, 
            new PaginationDetails(0, 2)
        );

        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(r => r.GetTicketsByCustomerEmail(email)).Returns(tickets.AsQueryable());
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        
        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(tickets.AsQueryable(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Ticket>>.Success(paginatedData));
        
        paginationServiceMock.Setup(p => p.MapData(paginatedData, It.IsAny<Func<Ticket, GetTicketForCustomerDto>>()))
            .Returns(mappedPaginatedData);
        
        var qrServiceMock = new Mock<IQRCodeService>();

        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);

        // Act
        var result = await sut.GetTicketsForCustomerAsync(email, page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mappedPaginatedData, result.Value);
        Assert.Equal(mappedData1, result.Value!.Data[0]);
        Assert.Equal(mappedData2, result.Value!.Data[1]);
    }

    [Fact]
    public async Task GetTicketsForCustomerAsync_WhenUserHasNoTickets_ReturnsEmptyPagination()
    {
        // Arrange
        var email = "empty@example.com";
        var page = 0;
        var pageSize = 10;
        
        var emptyTickets = new List<Ticket>();
        
        var emptyPaginatedData = new PaginatedData<Ticket>(emptyTickets, page, pageSize,  
            false, false, new PaginationDetails(0, 0));
        
        var paginatedResult = Result<PaginatedData<Ticket>>.Success(emptyPaginatedData);

        var mappedEmptyPaginatedData = new PaginatedData<GetTicketForCustomerDto>(new List<GetTicketForCustomerDto>(), 
            page, pageSize, false, false, new PaginationDetails(0, 0));

        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(r => r.GetTicketsByCustomerEmail(email)).Returns(emptyTickets.AsQueryable());
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock.Setup(p => p.PaginateAsync(emptyTickets.AsQueryable(), pageSize, page)).ReturnsAsync(paginatedResult);
        paginationServiceMock.Setup(p => p.MapData(emptyPaginatedData, It.IsAny<Func<Ticket, GetTicketForCustomerDto>>()))
            .Returns(mappedEmptyPaginatedData);
        
        var qrServiceMock = new Mock<IQRCodeService>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
    
        // Act
        var result = await sut.GetTicketsForCustomerAsync(email, page, pageSize);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Data);
    }

    [Fact]
    public async Task ScanTicket_WhenScanningSuccesful_ShouldReturnSuccess()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        ticketRepositoryMock.Setup(m => m.MarkTicketAsUsed(guid)).ReturnsAsync(Result.Success());
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
        // Act
        var res = await sut.ScanTicket(guid);
        
        // Assert
        Assert.True(res.IsSuccess);
    }
    
      [Fact]
    public async Task SetTicketForResellAsync_ReturnsFailure_WhenPriceIsZero()
    {
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);

        var result = await sut.SetTicketForResellAsync(Guid.NewGuid(), "test@example.com", 0, "zl");

        Assert.True(result.IsError);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task SetTicketForResellAsync_ReturnsFailure_WhenPriceTooHigh()
    {
        var ticket = CreateTicketForResellTest(price: 100);
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        ticketRepositoryMock
            .Setup(r => r.GetTicketWithDetailsByIdAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Ticket>.Success(ticket));
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        var result = await sut.SetTicketForResellAsync(Guid.NewGuid(), "test@example.com", 160, "zl");

        Assert.True(result.IsError);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task SetTicketForResellAsync_ReturnsFailure_WhenAlreadyForResell()
    {
        var ticket = CreateTicketForResellTest(price: 100, forResell: true);
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        ticketRepositoryMock
            .Setup(r => r.GetTicketWithDetailsByIdAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Ticket>.Success(ticket));
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        var result = await sut.SetTicketForResellAsync(Guid.NewGuid(), "test@example.com", 120, "zl");

        Assert.True(result.IsError);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task SetTicketForResellAsync_ReturnsFailure_WhenTicketIsUsed()
    {
        var ticket = CreateTicketForResellTest(price: 100, used: true);
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        
        ticketRepositoryMock
            .Setup(r => r.GetTicketWithDetailsByIdAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Ticket>.Success(ticket));
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);
        
        var result = await sut.SetTicketForResellAsync(Guid.NewGuid(), "test@example.com", 120, "zl");

        Assert.True(result.IsError);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task SetTicketForResellAsync_ReturnsSuccess_WhenValid()
    {
        var ticketRepositoryMock = new Mock<ITicketRepository>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var qrServiceMock = new Mock<IQRCodeService>();
        var ticket = CreateTicketForResellTest(price: 100);
        ticketRepositoryMock
            .Setup(r => r.GetTicketWithDetailsByIdAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Ticket>.Success(ticket));

        ticketRepositoryMock
            .Setup(r => r.SetTicketForResell(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success());
        
        var ticketTypeRepositoryMock = new Mock<ITicketTypeRepository>();
        var shoppingCartRepositoryMock = new Mock<IShoppingCartRepository>();
        var sut = new TicketService(ticketRepositoryMock.Object, ticketTypeRepositoryMock.Object,
            shoppingCartRepositoryMock.Object, paginationServiceMock.Object, qrServiceMock.Object);

        var result = await sut.SetTicketForResellAsync(Guid.NewGuid(), "test@example.com", 130, "zl");

        Assert.False(result.IsError);
    }

    private Ticket CreateTicketForResellTest(decimal price, bool forResell = false, bool used = false)
    {
        return new Ticket
        {
            Id = Guid.NewGuid(),
            Type = new TicketType { Price = price },
            ForResell = forResell,
            Used = used
        };
    }
}