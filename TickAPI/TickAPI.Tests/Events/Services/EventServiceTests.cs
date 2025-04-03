using Microsoft.AspNetCore.Http;
using TickAPI.Events.Abstractions;
using Moq;
using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Addresses.Models;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Services;

namespace TickAPI.Tests.Events.Services;

public class EventServiceTests
{
    [Fact]

    public async Task CreateNewEventAsync_WhenEventDataIsValid_ShouldReturnNewEvent()
    {
        // arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(e => e.AddNewEventAsync(It.IsAny<Event>())).Callback<Event>(e => e.Id = id)
            .Returns(Task.CompletedTask);
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(organizerEmail))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = organizerEmail, IsVerified = true }));
        
        var addressServiceMock = new Mock<IAddressService>();
        addressServiceMock.Setup(m => m.GetOrCreateAddressAsync(createAddress)).ReturnsAsync(
            Result<Address>.Success(new Address
            {
                City = createAddress.City,
                Country = createAddress.Country,
                FlatNumber = createAddress.FlatNumber,
                HouseNumber = createAddress.HouseNumber,
                PostalCode = createAddress.PostalCode,
                Street = createAddress.Street,
            })
        );
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(2003, 7, 11));

        var paginationServiceMock = new Mock<IPaginationService>();

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object);
        // act
        
        var result = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, eventStatus, organizerEmail);
        
        // assert
        
                
        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2025, 5, 1), result.Value!.StartDate);
        Assert.Equal(new DateTime(2025, 6, 1), result.Value!.EndDate);
        Assert.Equal(name, result.Value!.Name);
        Assert.Equal(description, result.Value!.Description);
        Assert.Equal(eventStatus, result.Value!.EventStatus);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal(organizerEmail, result.Value!.Organizer.Email);
    }
    
    [Fact]
    public async Task CreateNewEventAsync_WhenEndDateIsBeforeStartDate_ShouldReturnBadRequest()
    {
        // arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 8, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(organizerEmail))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = organizerEmail, IsVerified = true }));
        
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object);
        // act
        
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, eventStatus, organizerEmail);
        
        // assert
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("End date should be after start date", res.ErrorMsg);
    }
    
    [Fact]
    public async Task CreateNewEventAsync_WhenStartDateIsBeforeNow_ShouldReturnBadRequest()
    {
        // arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(organizerEmail))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = organizerEmail, IsVerified = true }));
        
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(2025, 5, 11));

        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object);
        // act
        
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, eventStatus, organizerEmail);
        
        // assert
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("Start date is in the past", res.ErrorMsg);
    }
}