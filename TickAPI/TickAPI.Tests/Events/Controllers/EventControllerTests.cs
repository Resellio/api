using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;
using System.Security.Claims;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Events.Controllers;
using TickAPI.Events.Abstractions;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Response;
using TickAPI.Organizers.Abstractions;

namespace TickAPI.Tests.Events.Controllers;

public class EventControllerTests
{
    [Fact]
    public async Task CreateEvent_WhenDataIsValid_ShouldReturnSuccess()
    {
        //arrange 
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string email = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        CreateEventDto eventDto = new CreateEventDto(name,  description, startDate,  endDate, minimumAge, eventStatus, createAddress);
        
        var eventServiceMock = new Mock<IEventService>();
        eventServiceMock
            .Setup(m => m.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, eventStatus, email))
            .ReturnsAsync(Result<Event>.Success(new Event()));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };

        var claimsServiceMock = new Mock<IClaimsService>();
        claimsServiceMock.Setup(m => m.GetEmailFromClaims(controllerContext.HttpContext.User.Claims)).Returns(Result<string>.Success(email));

        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);

        sut.ControllerContext = controllerContext;
        
        // act
        var res = await sut.CreateEvent(eventDto);
        
        // assert
        var result = Assert.IsType<ActionResult<CreateEventResponseDto>>(res);
        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal("Event created succesfully", objectResult.Value);
    }
    
    [Fact]
    public async Task CreateEvent_WhenMissingEmailClaims_ShouldReturnBadRequest()
    {
        //arrange 
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string email = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        
        var eventServiceMock = new Mock<IEventService>();
        var claimsServiceMock = new Mock<IClaimsService>();
        claimsServiceMock.Setup(m => m.GetEmailFromClaims(It.IsAny<IEnumerable<Claim>>())).Returns(Result<string>.Failure(StatusCodes.Status400BadRequest, "missing email claim"));

        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };
        
        // act
        var res = await sut.CreateEvent(new CreateEventDto(name, description, startDate, endDate, minimumAge, eventStatus, createAddress));
        
        // assert
        var result = Assert.IsType<ActionResult<CreateEventResponseDto>>(res);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("missing email claim", objectResult.Value);

    }
}