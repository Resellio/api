using Moq;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.Filters;
using TickAPI.Tickets.Models;

namespace TickAPI.Tests.Tickets.Filters;

public class TicketFilterApplierTests
{
    private readonly Mock<ITicketFilter> _mockTicketFilter;
    private readonly TicketFilterApplier _ticketFilterApplier;
    private readonly IQueryable<Ticket> _emptyQueryable = new List<Ticket>().AsQueryable();

    public TicketFilterApplierTests()
    {
        _mockTicketFilter = new Mock<ITicketFilter>();
        _mockTicketFilter.Setup(tf => tf.GetTickets()).Returns(_emptyQueryable);
        _ticketFilterApplier = new TicketFilterApplier(_mockTicketFilter.Object);
    }

    [Fact]
    public void ApplyFilters_WithEventName_ShouldCallFilterTicketsByEventName()
    {
        // Arrange
        var filters = new TicketFiltersDto
        {
            EventName = "concert",
            usedOnly = false,
            unusedOnly = false,
            forResellOnly = false,
            notForResellOnly = false
        };

        // Act
        _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterTicketsByEventName(filters.EventName!), Times.Once);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithUsedOnly_ShouldCallFilterUsedTickets()
    {
        // Arrange
        var filters = new TicketFiltersDto
        {
            EventName = null,
            usedOnly = true,
            unusedOnly = false,
            forResellOnly = false,
            notForResellOnly = false
        };

        // Act
        _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterUsedTickets(), Times.Once);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithUnusedOnly_ShouldCallFilterUnusedTickets()
    {
        // Arrange
        var filters = new TicketFiltersDto
        {
            EventName = null,
            usedOnly = false,
            unusedOnly = true,
            forResellOnly = false,
            notForResellOnly = false
        };

        // Act
        _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterUnusedTickets(), Times.Once);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithForResellOnly_ShouldCallFilterTicketsForResell()
    {
        // Arrange
        var filters = new TicketFiltersDto
        {
            EventName = null,
            usedOnly = false,
            unusedOnly = false,
            forResellOnly = true,
            notForResellOnly = false
        };

        // Act
        _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterTicketsForResell(), Times.Once);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithNotForResellOnly_ShouldCallFilterTicketsNotForResell()
    {
        // Arrange
        var filters = new TicketFiltersDto
        {
            EventName = null,
            usedOnly = false,
            unusedOnly = false,
            forResellOnly = false,
            notForResellOnly = true
        };

        // Act
        _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterTicketsNotForResell(), Times.Once);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMultipleFilters_ShouldCallAllRelevantFilters()
    {
        // Arrange
        var filters = new TicketFiltersDto
        {
            EventName = "concert",
            usedOnly = true,
            unusedOnly = false,
            forResellOnly = true,
            notForResellOnly = false
        };

        // Act
        _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterTicketsByEventName(filters.EventName!), Times.Once);
        _mockTicketFilter.Verify(tf => tf.FilterUsedTickets(), Times.Once);
        _mockTicketFilter.Verify(tf => tf.FilterTicketsForResell(), Times.Once);
        _mockTicketFilter.Verify(tf => tf.FilterUnusedTickets(), Times.Never);
        _mockTicketFilter.Verify(tf => tf.FilterTicketsNotForResell(), Times.Never);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithNoFilters_ShouldOnlyCallGetTickets()
    {
        // Arrange
        var expectedResult = new List<Ticket> 
        { 
            new Ticket { NameOnTicket = "Test Ticket" } 
        }.AsQueryable();
        _mockTicketFilter.Setup(tf => tf.GetTickets()).Returns(expectedResult);
        var filters = new TicketFiltersDto
        {
            EventName = null,
            usedOnly = false,
            unusedOnly = false,
            forResellOnly = false,
            notForResellOnly = false
        };

        // Act
        var result = _ticketFilterApplier.ApplyFilters(filters);

        // Assert
        _mockTicketFilter.Verify(tf => tf.FilterTicketsByEventName(It.IsAny<string>()), Times.Never);
        _mockTicketFilter.Verify(tf => tf.FilterUsedTickets(), Times.Never);
        _mockTicketFilter.Verify(tf => tf.FilterUnusedTickets(), Times.Never);
        _mockTicketFilter.Verify(tf => tf.FilterTicketsForResell(), Times.Never);
        _mockTicketFilter.Verify(tf => tf.FilterTicketsNotForResell(), Times.Never);
        _mockTicketFilter.Verify(tf => tf.GetTickets(), Times.Once);
        Assert.Same(expectedResult, result);
    }
    
}