using System.Runtime.CompilerServices;
using TickAPI.Events.Abstractions;
using TickAPI.Events.DTOs.Request;
using Moq;
using TickAPI.Events.Models;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.Services;

namespace TickAPI.Tests.Events.Services;

public class EventServicesTests
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
        addressServiceMock.Setup(m => m.GetAddressAsync(createAddress)).ReturnsAsync(
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

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object);
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
}