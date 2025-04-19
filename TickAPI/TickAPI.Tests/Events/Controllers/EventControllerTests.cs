using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;
using System.Security.Claims;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Categories.DTOs.Request;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Events.Controllers;
using TickAPI.Events.Abstractions;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Response;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;
using TickAPI.TicketTypes.DTOs.Request;

namespace TickAPI.Tests.Events.Controllers;

public class EventControllerTests
{
    [Fact]
    public async Task CreateEvent_WhenDataIsValid_ShouldReturnSuccess()
    {
        // Arrange 
        const string name = "Concert";
        const string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        const string email = "123@mail.com";
        const EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        List<CreateEventCategoryDto> categories =
        [
            new CreateEventCategoryDto("concert"),
            new CreateEventCategoryDto("bear metal")
        ];
        List<CreateEventTicketTypeDto> ticketTypes =
        [
            new CreateEventTicketTypeDto("normal", 100, 50.9m, "zł",  new DateTime(2025, 5, 1)),
            new CreateEventTicketTypeDto("V.I.P", 10, 500.9m, "zł",  new DateTime(2025, 5, 10)),
        ];
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        CreateEventDto eventDto = new CreateEventDto(name,  description, startDate,  endDate, minimumAge, categories, ticketTypes, eventStatus, createAddress);
        
        var eventServiceMock = new Mock<IEventService>();
        eventServiceMock
            .Setup(m => m.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories , ticketTypes, eventStatus, email))
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
        
        // Act
        var res = await sut.CreateEvent(eventDto);
        
        // Assert
        var result = Assert.IsType<ActionResult<CreateEventResponseDto>>(res);
        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal("Event created succesfully", objectResult.Value);
    }
    
    [Fact]
    public async Task CreateEvent_WhenMissingEmailClaims_ShouldReturnBadRequest()
    {
        // Arrange 
        const string name = "Concert";
        const string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        const EventStatus eventStatus = EventStatus.TicketsAvailable;
        string email = "123@mail.com";
        List<CreateEventCategoryDto> categories =
        [
            new CreateEventCategoryDto("concert"),
            new CreateEventCategoryDto("bear metal")
        ];
        List<CreateEventTicketTypeDto> ticketTypes =
        [
            new CreateEventTicketTypeDto("normal", 100, 50.9m, "zł",  new DateTime(2025, 5, 1)),
            new CreateEventTicketTypeDto("V.I.P", 10, 500.9m, "zł",  new DateTime(2025, 5, 10)),
        ];
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
        var res = await sut.CreateEvent(new CreateEventDto(name, description, startDate, endDate, minimumAge, categories, ticketTypes, eventStatus, createAddress));
        
        // Assert
        var result = Assert.IsType<ActionResult<CreateEventResponseDto>>(res);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("missing email claim", objectResult.Value);
    }
    
    [Fact]
    public async Task GetOrganizerEvents_WhenAllOperationsSucceed_ShouldReturnOkWithPaginatedData()
    {
        // Arrange
        const string email = "organizer@example.com";
        const int page = 0;
        const int pageSize = 10;
        
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
        claimsServiceMock
            .Setup(m => m.GetEmailFromClaims(controllerContext.HttpContext.User.Claims))
            .Returns(Result<string>.Success(email));
        
        var organizer = new Organizer { Email = email, IsVerified = true };
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var paginatedData = new PaginatedData<GetEventResponseDto>(
            new List<GetEventResponseDto>
            {
                Utils.CreateSampleEventResponseDto("Event 1"),
                Utils.CreateSampleEventResponseDto("Event 2")
            },
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 2)
        );
        
        var eventServiceMock = new Mock<IEventService>();
        eventServiceMock
            .Setup(m => m.GetOrganizerEventsAsync(organizer, page, pageSize))
            .ReturnsAsync(Result<PaginatedData<GetEventResponseDto>>.Success(paginatedData));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        sut.ControllerContext = controllerContext;
        
        // Act
        var response = await sut.GetOrganizerEvents(pageSize, page);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetEventResponseDto>>>(response);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        
        var returnedPaginatedData = Assert.IsType<PaginatedData<GetEventResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedPaginatedData.Data.Count);
        Assert.Equal(paginatedData.Data[0], returnedPaginatedData.Data[0]);
        Assert.Equal(paginatedData.Data[1], returnedPaginatedData.Data[1]);
        Assert.Equal(page, returnedPaginatedData.PageNumber);
        Assert.Equal(pageSize, returnedPaginatedData.PageSize);
        Assert.False(returnedPaginatedData.HasNextPage);
        Assert.False(returnedPaginatedData.HasPreviousPage);
    }
    
    [Fact]
    public async Task GetOrganizerEvents_WhenEmailClaimIsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;
        const string errorMessage = "Missing email claim";
        
        var claimsServiceMock = new Mock<IClaimsService>();
        claimsServiceMock
            .Setup(m => m.GetEmailFromClaims(It.IsAny<IEnumerable<Claim>>()))
            .Returns(Result<string>.Failure(StatusCodes.Status400BadRequest, errorMessage));
        
        var eventServiceMock = new Mock<IEventService>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };
        
        // Act
        var response = await sut.GetOrganizerEvents(pageSize, page);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetEventResponseDto>>>(response);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(errorMessage, objectResult.Value);
    }
    
    [Fact]
    public async Task GetOrganizerEvents_WhenOrganizerIsNotFound_ShouldReturnNotFound()
    {
        // Arrange
        const string email = "organizer@example.com";
        const int page = 0;
        const int pageSize = 10;
        const string errorMessage = "Organizer not found";
        
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
        claimsServiceMock
            .Setup(m => m.GetEmailFromClaims(controllerContext.HttpContext.User.Claims))
            .Returns(Result<string>.Success(email));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Failure(StatusCodes.Status404NotFound, errorMessage));
        
        var eventServiceMock = new Mock<IEventService>();
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        sut.ControllerContext = controllerContext;
        
        // Act
        var response = await sut.GetOrganizerEvents(pageSize, page);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetEventResponseDto>>>(response);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        Assert.Equal(errorMessage, objectResult.Value);
    }
    
    [Fact]
    public async Task GetOrganizerEvents_WhenPaginationFails_ShouldReturnBadRequest()
    {
        // Arrange
        const string email = "organizer@example.com";
        const int page = -1; // Invalid page
        const int pageSize = 10;
        const string errorMessage = "Invalid page number";
        
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
        claimsServiceMock
            .Setup(m => m.GetEmailFromClaims(controllerContext.HttpContext.User.Claims))
            .Returns(Result<string>.Success(email));
        
        var organizer = new Organizer { Email = email, IsVerified = true };
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var eventServiceMock = new Mock<IEventService>();
        eventServiceMock
            .Setup(m => m.GetOrganizerEventsAsync(organizer, page, pageSize))
            .ReturnsAsync(Result<PaginatedData<GetEventResponseDto>>.Failure(StatusCodes.Status400BadRequest, errorMessage));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        sut.ControllerContext = controllerContext;
        
        // Act
        var response = await sut.GetOrganizerEvents(pageSize, page);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetEventResponseDto>>>(response);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(errorMessage, objectResult.Value);
    }
    
    [Fact]
    public async Task GetOrganizerEventsPaginationDetails_WhenAllOperationsSucceed_ShouldReturnOkWithPaginationDetails()
    {
        // Arrange
        const string email = "organizer@example.com";
        const int pageSize = 10;
        
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
        claimsServiceMock
            .Setup(m => m.GetEmailFromClaims(controllerContext.HttpContext.User.Claims))
            .Returns(Result<string>.Success(email));
        
        var organizer = new Organizer { Email = email, IsVerified = true };
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var paginationDetails = new PaginationDetails(2, 25);
        
        var eventServiceMock = new Mock<IEventService>();
        eventServiceMock
            .Setup(m => m.GetOrganizerEventsPaginationDetailsAsync(organizer, pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Success(paginationDetails));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        sut.ControllerContext = controllerContext;
        
        // Act
        var response = await sut.GetOrganizerEventsPaginationDetails(pageSize);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginationDetails>>(response);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        
        var returnedPaginationDetails = Assert.IsType<PaginationDetails>(okResult.Value);
        Assert.Equal(2, returnedPaginationDetails.MaxPageNumber);
        Assert.Equal(25, returnedPaginationDetails.AllElementsCount);
    }
    
    [Fact]
    public async Task GetOrganizerEventsPaginationDetails_WhenPaginationDetailsFails_ShouldReturnBadRequest()
    {
        // Arrange
        const string email = "organizer@example.com";
        const int pageSize = -1;
        const string errorMessage = "Invalid page size";
        
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
        claimsServiceMock
            .Setup(m => m.GetEmailFromClaims(controllerContext.HttpContext.User.Claims))
            .Returns(Result<string>.Success(email));
        
        var organizer = new Organizer { Email = email, IsVerified = true };
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var eventServiceMock = new Mock<IEventService>();
        eventServiceMock
            .Setup(m => m.GetOrganizerEventsPaginationDetailsAsync(organizer, pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Failure(StatusCodes.Status400BadRequest, errorMessage));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        sut.ControllerContext = controllerContext;
        
        // Act
        var response = await sut.GetOrganizerEventsPaginationDetails(pageSize);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginationDetails>>(response);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(errorMessage, objectResult.Value);
    }
    
    [Fact]
    public async Task GetEvents_WhenAllOperationsSucceed_ShouldReturnOkWithPaginatedData()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;
        
        var eventServiceMock = new Mock<IEventService>();
        var claimsServiceMock = new Mock<IClaimsService>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var paginatedData = new PaginatedData<GetEventResponseDto>(
            new List<GetEventResponseDto>
            {
                Utils.CreateSampleEventResponseDto("Event 1"),
                Utils.CreateSampleEventResponseDto("Event 2")
            },
            page,
            pageSize,
            false,
            false,
            new PaginationDetails(0, 2)
        );
        
        eventServiceMock
            .Setup(m => m.GetEventsAsync(page, pageSize))
            .ReturnsAsync(Result<PaginatedData<GetEventResponseDto>>.Success(paginatedData));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        
        // Act
        var response = await sut.GetEvents(pageSize, page);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetEventResponseDto>>>(response);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        
        var returnedPaginatedData = Assert.IsType<PaginatedData<GetEventResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedPaginatedData.Data.Count);
        Assert.Equal(paginatedData.Data[0], returnedPaginatedData.Data[0]);
        Assert.Equal(paginatedData.Data[1], returnedPaginatedData.Data[1]);
        Assert.Equal(page, returnedPaginatedData.PageNumber);
        Assert.Equal(pageSize, returnedPaginatedData.PageSize);
        Assert.False(returnedPaginatedData.HasNextPage);
        Assert.False(returnedPaginatedData.HasPreviousPage);
    }

    [Fact]
    public async Task GetEvents_WhenOperationFails_ShouldReturnErrorWithCorrectStatusCode()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;
        const string errorMessage = "Failed to retrieve events";
        const int statusCode = StatusCodes.Status500InternalServerError;
        
        var eventServiceMock = new Mock<IEventService>();
        var claimsServiceMock = new Mock<IClaimsService>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        eventServiceMock
            .Setup(m => m.GetEventsAsync(page, pageSize))
            .ReturnsAsync(Result<PaginatedData<GetEventResponseDto>>.Failure(statusCode, errorMessage));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        
        // Act
        var response = await sut.GetEvents(pageSize, page);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginatedData<GetEventResponseDto>>>(response);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(statusCode, objectResult.StatusCode);
        Assert.Equal(errorMessage, objectResult.Value);
    }
    
    [Fact]
    public async Task GetEventsPaginationDetails_WhenAllOperationsSucceed_ShouldReturnOkWithPaginationDetails()
    {
        // Arrange
        const int pageSize = 10;
        
        var eventServiceMock = new Mock<IEventService>();
        var claimsServiceMock = new Mock<IClaimsService>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var paginationDetails = new PaginationDetails(0, 20);
        
        eventServiceMock
            .Setup(m => m.GetEventsPaginationDetailsAsync(pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Success(paginationDetails));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        
        // Act
        var response = await sut.GetEventsPaginationDetails(pageSize);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginationDetails>>(response);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        
        var returnedPaginationDetails = Assert.IsType<PaginationDetails>(okResult.Value);
        Assert.Equal(paginationDetails.AllElementsCount, returnedPaginationDetails.AllElementsCount);
        Assert.Equal(paginationDetails.MaxPageNumber, returnedPaginationDetails.MaxPageNumber);
    }

    [Fact]
    public async Task GetEventsPaginationDetails_WhenOperationFails_ShouldReturnErrorWithCorrectStatusCode()
    {
        // Arrange
        const int pageSize = 10;
        const string errorMessage = "Failed to retrieve pagination details";
        const int statusCode = StatusCodes.Status500InternalServerError;
        
        var eventServiceMock = new Mock<IEventService>();
        var claimsServiceMock = new Mock<IClaimsService>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        eventServiceMock
            .Setup(m => m.GetEventsPaginationDetailsAsync(pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Failure(statusCode, errorMessage));
        
        var sut = new EventController(eventServiceMock.Object, claimsServiceMock.Object, organizerServiceMock.Object);
        
        // Act
        var response = await sut.GetEventsPaginationDetails(pageSize);
        
        // Assert
        var result = Assert.IsType<ActionResult<PaginationDetails>>(response);
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(statusCode, objectResult.StatusCode);
        Assert.Equal(errorMessage, objectResult.Value);
}
}