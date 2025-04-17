using Microsoft.AspNetCore.Http;
using TickAPI.Events.Abstractions;
using Moq;
using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Addresses.Models;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Categories.Abstractions;
using TickAPI.Categories.DTOs.Request;
using TickAPI.Categories.Models;
using TickAPI.Events.Models;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.Services;
using TickAPI.TicketTypes.DTOs.Request;
using TickAPI.TicketTypes.Models;

namespace TickAPI.Tests.Events.Services;

public class EventServiceTests
{
    [Fact]

    public async Task CreateNewEventAsync_WhenEventDataIsValid_ShouldReturnNewEvent()
    {
        // Arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        List<CreateEventCategoryDto> categories =
        [
            new CreateEventCategoryDto("concert"),
            new CreateEventCategoryDto("bear metal")
        ];
        List<Category> expectedCategories =
        [
            new Category
            {
                Name = "concert",
            },
            new Category
            {
                Name = "bear metal",
            }
            
        ];
        List<CreateEventTicketTypeDto> ticketTypes =
        [
            new CreateEventTicketTypeDto("normal", 100, 50.9m, "zł",  new DateTime(2025, 5, 1)),
            new CreateEventTicketTypeDto("V.I.P", 10, 500.9m, "zł",  new DateTime(2025, 5, 10)),
        ];
        List<TicketType> expectedTicketTypes =
        [
            new TicketType
            {
                Description = "normal",
                MaxCount = 100,
                Price = 50.9m,
                Currency = "zł",
                AvailableFrom = new DateTime(2025, 5, 1)
            },
            new TicketType
            {
                Description = "V.I.P",
                MaxCount = 10,
                Price = 500.9m,
                Currency = "zł",
                AvailableFrom = new DateTime(2025, 5, 10)
            },
        ];
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
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        categoryRepositoryMock.Setup(c => c.CheckIfCategoriesExistAsync(It.IsAny<List<Category>>())).Returns(Task.FromResult(true));

        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);
        // act
        var result = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories, ticketTypes, eventStatus, organizerEmail);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2025, 5, 1), result.Value!.StartDate);
        Assert.Equal(new DateTime(2025, 6, 1), result.Value!.EndDate);
        Assert.Equal(name, result.Value!.Name);
        Assert.Equal(description, result.Value!.Description);
        Assert.Equal(eventStatus, result.Value!.EventStatus);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal(organizerEmail, result.Value!.Organizer.Email);
        Assert.Equal(expectedCategories.Count, result.Value!.Categories.Count);
        foreach (var expectedCategory in expectedCategories)
        {
            Assert.Contains(result.Value!.Categories, actualCategory => 
                actualCategory.Name == expectedCategory.Name);
        }
        Assert.Equal(expectedTicketTypes.Count, result.Value!.TicketTypes.Count);
        foreach (var expectedTicketType in expectedTicketTypes)
        {
            Assert.Contains(result.Value!.TicketTypes, actualTicketType => 
                actualTicketType.Description == expectedTicketType.Description &&
                actualTicketType.MaxCount == expectedTicketType.MaxCount &&
                actualTicketType.Price == expectedTicketType.Price &&
                actualTicketType.Currency == expectedTicketType.Currency &&
                actualTicketType.AvailableFrom == expectedTicketType.AvailableFrom);
        }
    }
    
    [Fact]
    public async Task CreateNewEventAsync_WhenEndDateIsBeforeStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 8, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
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
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(organizerEmail))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = organizerEmail, IsVerified = true }));
        
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        
        
        // act
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);
        
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge,  createAddress, categories, ticketTypes, eventStatus, organizerEmail);
        
        // Assert
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("End date should be after start date", res.ErrorMsg);
    }
    
    [Fact]
    public async Task CreateNewEventAsync_WhenTicketTypeAvailabilityIsAfterEventsEnd_ShouldReturnBadRequest()
    {
        // Arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
        Guid id = Guid.NewGuid();
        List<CreateEventCategoryDto> categories =
        [
            new CreateEventCategoryDto("concert"),
            new CreateEventCategoryDto("bear metal")
        ];
        List<CreateEventTicketTypeDto> ticketTypes =
        [
            new CreateEventTicketTypeDto("normal", 100, 50.9m, "zł",  new DateTime(2025, 5, 1)),
            new CreateEventTicketTypeDto("V.I.P", 10, 500.9m, "zł",  new DateTime(2025, 6, 10)),
        ];
        CreateAddressDto createAddress = new CreateAddressDto("United States", "New York", "Main st", 20, null, "00-000");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(organizerEmail))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = organizerEmail, IsVerified = true }));
        
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(2025, 4, 11));
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        
        // act
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);
        
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories, ticketTypes, eventStatus, organizerEmail);
        
        // Assert
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("Tickets can't be available after the event is over", res.ErrorMsg);
    }
    
    [Fact]
    public async Task CreateNewEventAsync_WhenStartDateIsBeforeNow_ShouldReturnBadRequest()
    {
        // Arrange
        string name = "Concert";
        string description = "Description of a concert";
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 6, 1);
        uint? minimumAge = 18;
        string organizerEmail = "123@mail.com";
        EventStatus eventStatus = EventStatus.TicketsAvailable;
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
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(organizerEmail))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = organizerEmail, IsVerified = true }));
        
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(2025, 5, 11));
        
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        
        // act
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);
        
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories, ticketTypes, eventStatus, organizerEmail);
        
        // Assert
        Assert.False(res.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("Start date is in the past", res.ErrorMsg);
    }
    
    [Fact]
    public async Task GetOrganizerEvents_WhenPaginationSucceeds_ShouldReturnPaginatedEvents()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true,
            Events = new List<Event>
            {
                Utils.CreateSampleEvent("Event 1"),
                Utils.CreateSampleEvent("Event 2"),
                Utils.CreateSampleEvent("Event 3")
            }
        };
        int page = 0;
        int pageSize = 2;

        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var paginatedEvents = new PaginatedData<Event>(
            organizer.Events.Take(pageSize).ToList(),
            page,
            pageSize,
            true,
            false,
            new PaginationDetails(1, 3)
        );

        var organizerEvents = organizer.Events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsByOranizer(organizer)).Returns(organizerEvents);
        
        paginationServiceMock
            .Setup(p => p.PaginateAsync(organizerEvents, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Event>>.Success(paginatedEvents));

        paginationServiceMock
            .Setup(p => p.MapData(paginatedEvents, It.IsAny<Func<Event, GetEventResponseDto>>()))
            .Returns(new PaginatedData<GetEventResponseDto>(
                new List<GetEventResponseDto>
                {
                    Utils.CreateSampleEventResponseDto("Event 1"),
                    Utils.CreateSampleEventResponseDto("Event 2")
                },
                page,
                pageSize,
                true,
                false,
                new PaginationDetails(1, 3)
            ));

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, 
            dateTimeServiceMock.Object, paginationServiceMock.Object,  categoryRepositoryMock.Object);

        // Act
        var result = await sut.GetOrganizerEventsAsync(organizer, page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Data.Count);
        Assert.Equal("Event 1", result.Value!.Data[0].Name);
        Assert.Equal("Event 2", result.Value!.Data[1].Name);
        Assert.Equal(0, result.Value!.PageNumber);
        Assert.Equal(2, result.Value!.PageSize);
        Assert.True(result.Value!.HasNextPage);
        Assert.False(result.Value!.HasPreviousPage);
        Assert.Equal(1, result.Value!.PaginationDetails.MaxPageNumber);
        Assert.Equal(3, result.Value!.PaginationDetails.AllElementsCount);
    }
    
    [Fact]
    public async Task GetOrganizerEvents_WhenPaginationFails_ShouldPropagateError()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true,
            Events = new List<Event>
            {
                Utils.CreateSampleEvent("Event 1"),
                Utils.CreateSampleEvent("Event 2")
            }
        };
        int page = 2; // Invalid page
        int pageSize = 2;

        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var organizerEvents = organizer.Events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsByOranizer(organizer)).Returns(organizerEvents);
        
        paginationServiceMock
            .Setup(p => p.PaginateAsync(organizerEvents, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Event>>.Failure(StatusCodes.Status400BadRequest, "Invalid page number"));

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, 
            dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);

        // Act
        var result = await sut.GetOrganizerEventsAsync(organizer, page, pageSize);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Invalid page number", result.ErrorMsg);
    }
    
    [Fact]
    public async Task GetEventsAsync_WhenPaginationSucceeds_ShouldReturnPaginatedEvents()
    {
        // Arrange
        var events = new List<Event>
        {
            Utils.CreateSampleEvent("Event 1"),
            Utils.CreateSampleEvent("Event 2"),
            Utils.CreateSampleEvent("Event 3")
        };
        int page = 0;
        int pageSize = 2;

        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var paginatedEvents = new PaginatedData<Event>(
            events.Take(pageSize).ToList(),
            page,
            pageSize,
            true,
            false,
            new PaginationDetails(1, 3)
        );

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEvents()).Returns(eventsQueryable);
        
        paginationServiceMock
            .Setup(p => p.PaginateAsync(eventsQueryable, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Event>>.Success(paginatedEvents));

        paginationServiceMock
            .Setup(p => p.MapData(paginatedEvents, It.IsAny<Func<Event, GetEventResponseDto>>()))
            .Returns(new PaginatedData<GetEventResponseDto>(
                new List<GetEventResponseDto>
                {
                    Utils.CreateSampleEventResponseDto("Event 1"),
                    Utils.CreateSampleEventResponseDto("Event 2")
                },
                page,
                pageSize,
                true,
                false,
                new PaginationDetails(1, 3)
            ));

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, 
            dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);

        // Act
        var result = await sut.GetEventsAsync(page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Data.Count);
        Assert.Equal("Event 1", result.Value!.Data[0].Name);
        Assert.Equal("Event 2", result.Value!.Data[1].Name);
        Assert.Equal(0, result.Value!.PageNumber);
        Assert.Equal(2, result.Value!.PageSize);
        Assert.True(result.Value!.HasNextPage);
        Assert.False(result.Value!.HasPreviousPage);
        Assert.Equal(1, result.Value!.PaginationDetails.MaxPageNumber);
        Assert.Equal(3, result.Value!.PaginationDetails.AllElementsCount);
    }

    [Fact]
    public async Task GetEventsAsync_WhenPaginationFails_ShouldPropagateError()
    {
        // Arrange
        var events = new List<Event>
        {
            Utils.CreateSampleEvent("Event 1"),
            Utils.CreateSampleEvent("Event 2")
        };
        int page = 2; // Invalid page
        int pageSize = 2;

        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEvents()).Returns(eventsQueryable);
        
        paginationServiceMock
            .Setup(p => p.PaginateAsync(eventsQueryable, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Event>>.Failure(StatusCodes.Status400BadRequest, "Invalid page number"));

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, 
            dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);

        // Act
        var result = await sut.GetEventsAsync(page, pageSize);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Invalid page number", result.ErrorMsg);
    }

    [Fact]
    public async Task GetEventsPaginationDetailsAsync_WhenSuccessful_ShouldReturnPaginationDetails()
    {
        // Arrange
        var events = new List<Event>
        {
            Utils.CreateSampleEvent("Event 1"),
            Utils.CreateSampleEvent("Event 2"),
            Utils.CreateSampleEvent("Event 3")
        };
        int pageSize = 2;

        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEvents()).Returns(eventsQueryable);
        
        var paginationDetails = new PaginationDetails(1, 3);
        paginationServiceMock
            .Setup(p => p.GetPaginationDetailsAsync(eventsQueryable, pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Success(paginationDetails));

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, 
            dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);

        // Act
        var result = await sut.GetEventsPaginationDetailsAsync(pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.MaxPageNumber);
        Assert.Equal(3, result.Value!.AllElementsCount);
    }

    [Fact]
    public async Task GetEventsPaginationDetailsAsync_WhenFails_ShouldReturnError()
    {
        // Arrange
        var events = new List<Event>
        {
            Utils.CreateSampleEvent("Event 1"),
            Utils.CreateSampleEvent("Event 2")
        };
        int pageSize = -1; // Invalid page size

        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEvents()).Returns(eventsQueryable);
        
        paginationServiceMock
            .Setup(p => p.GetPaginationDetailsAsync(eventsQueryable, pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Failure(StatusCodes.Status400BadRequest, "Invalid page size"));

        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, 
            dateTimeServiceMock.Object, paginationServiceMock.Object, categoryRepositoryMock.Object);

        // Act
        var result = await sut.GetEventsPaginationDetailsAsync(pageSize);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Invalid page size", result.ErrorMsg);
    }
}