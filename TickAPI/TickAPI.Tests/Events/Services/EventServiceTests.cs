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
using TickAPI.Common.Blob.Abstractions;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Events.Models;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Customers.Abstractions;
using TickAPI.Events.DTOs.Response;
using TickAPI.Events.Services;
using TickAPI.Tickets.Abstractions;
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
        
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock.Setup(c => c.GetCategoriesByNames(It.IsAny<List<string>>())).Returns(Result<List<Category>>.Success(expectedCategories));

        var paginationServiceMock = new Mock<IPaginationService>();
        
        var ticketServiceMock = new Mock<ITicketService>();

        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);
        // Act
        var result = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories, ticketTypes, eventStatus, organizerEmail, null);
        
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
        
        var categoryServiceMock = new Mock<ICategoryService>();

        var paginationServiceMock = new Mock<IPaginationService>();
        
        var ticketServiceMock = new Mock<ITicketService>();
        
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);
        
        // Act
        
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge,  createAddress, categories, ticketTypes, eventStatus, organizerEmail, null);
        
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
        
        var categoryServiceMock = new Mock<ICategoryService>();
        
        
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var ticketServiceMock = new Mock<ITicketService>();
        
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);
        
        // Act
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories, ticketTypes, eventStatus, organizerEmail, null);
        
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
        
        var categoryServiceMock = new Mock<ICategoryService>();
        
        var paginationServiceMock = new Mock<IPaginationService>();
        
        var ticketServiceMock = new Mock<ITicketService>();
        
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);
        
        // Act
        var res = await sut.CreateNewEventAsync(name, description, startDate, endDate, minimumAge, createAddress, categories, ticketTypes, eventStatus, organizerEmail, null);
        
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
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();

        var paginatedEvents = new PaginatedData<Event>(
            organizer.Events.Take(pageSize).ToList(),
            page,
            pageSize,
            true,
            false,
            new PaginationDetails(1, 3)
        );

        var organizerEvents = organizer.Events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsByOranizerAsync(organizer)).ReturnsAsync(organizerEvents);
        
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
        
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);

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
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();

        var organizerEvents = organizer.Events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsByOranizerAsync(organizer)).ReturnsAsync(organizerEvents);
        
        paginationServiceMock
            .Setup(p => p.PaginateAsync(organizerEvents, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Event>>.Failure(StatusCodes.Status400BadRequest, "Invalid page number"));

        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);

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
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();

        var paginatedEvents = new PaginatedData<Event>(
            events.Take(pageSize).ToList(),
            page,
            pageSize,
            true,
            false,
            new PaginationDetails(1, 3)
        );

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsAsync()).ReturnsAsync(eventsQueryable);
        
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

        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);
        
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
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsAsync()).ReturnsAsync(eventsQueryable);
        
        paginationServiceMock
            .Setup(p => p.PaginateAsync(eventsQueryable, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Event>>.Failure(StatusCodes.Status400BadRequest, "Invalid page number"));

        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);

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
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsAsync()).ReturnsAsync(eventsQueryable);
        
        var paginationDetails = new PaginationDetails(1, 3);
        paginationServiceMock
            .Setup(p => p.GetPaginationDetailsAsync(eventsQueryable, pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Success(paginationDetails));

        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);
        
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
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();

        var eventsQueryable = events.AsQueryable();
        eventRepositoryMock.Setup(p => p.GetEventsAsync()).ReturnsAsync(eventsQueryable);
        
        paginationServiceMock
            .Setup(p => p.GetPaginationDetailsAsync(eventsQueryable, pageSize))
            .ReturnsAsync(Result<PaginationDetails>.Failure(StatusCodes.Status400BadRequest, "Invalid page size"));

        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.GetEventsPaginationDetailsAsync(pageSize);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Invalid page size", result.ErrorMsg);
    }

    [Fact]
    public async Task GetEventDetailsAsync_WhenSuccessful_ShouldReturnEventDetails()
    {
        // Arrange
        var @event = Utils.CreateSampleEvent("Event");
        var expectedDetails = Utils.CreateSampleEventDetailsDto("Event");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();
        
        eventRepositoryMock
            .Setup(m => m.GetEventByIdAsync(@event.Id))
            .ReturnsAsync(Result<Event>.Success(@event));

        ticketServiceMock
            .Setup(m => m.GetNumberOfAvailableTicketsByTypeAsync(It.IsAny<TicketType>()))
            .ReturnsAsync((TicketType input) => 
                Result<uint>.Success((uint)(input.Price / 10))
            );
        
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.GetEventDetailsAsync(@event.Id);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedDetails.Id, result.Value!.Id);
        Assert.Equal(expectedDetails.Description, result.Value!.Description);
        Assert.Equal(expectedDetails.StartDate, result.Value!.StartDate);
        Assert.Equal(expectedDetails.EndDate, result.Value!.EndDate);
        Assert.Equal(expectedDetails.MinimumAge, result.Value!.MinimumAge);
        Assert.True(expectedDetails.Categories.SequenceEqual(result.Value!.Categories));
        Assert.True(expectedDetails.TicketTypes.SequenceEqual(result.Value!.TicketTypes));
        Assert.Equal(expectedDetails.Status, result.Value!.Status);
        Assert.Equal(expectedDetails.Address, result.Value!.Address);
    }
    
    [Fact]
    public async Task GetEventDetailsAsync_WhenFails_ShouldReturnEventError()
    {
        // Arrange
        var @event = Utils.CreateSampleEvent("Event");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var categoryServiceMock = new Mock<ICategoryService>();
        var ticketServiceMock = new Mock<ITicketService>();
        
        eventRepositoryMock
            .Setup(m => m.GetEventByIdAsync(@event.Id))
            .ReturnsAsync(Result<Event>.Failure(StatusCodes.Status404NotFound, $"event with id {@event.Id} not found"));
        
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.GetEventDetailsAsync(@event.Id);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal($"event with id {@event.Id} not found", result.ErrorMsg);
    }
    
    [Fact]
    public async Task EditEventAsync_WhenDataValid_ShouldUpdateEvent()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description of a concert";
        DateTime startDate = new DateTime(2025, 6, 1);
        DateTime endDate = new DateTime(2025, 7, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Description = "Original description",
            StartDate = new DateTime(2025, 5, 1),
            EndDate = new DateTime(2025, 5, 15),
            MinimumAge = 18,
            EventStatus = EventStatus.TicketsAvailable,
            Organizer = organizer,
            Categories = new List<Category>
            {
                new Category { Name = "rock" }
            },
            Address = new Address
            {
                Country = "United States",
                City = "Chicago",
                Street = "State st",
                HouseNumber = 100,
                FlatNumber = null,
                PostalCode = "60001"
            },
            TicketTypes = [],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz"),
            new EditEventCategoryDto("classical")
        ];
        
        List<Category> expectedCategories =
        [
            new Category { Name = "jazz" },
            new Category { Name = "classical" }
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        var updatedAddress = new Address
        {
            Country = "United States",
            City = "New York",
            Street = "Broadway",
            HouseNumber = 42,
            FlatNumber = null,
            PostalCode = "10001"
        };
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        eventRepositoryMock
            .Setup(e => e.SaveEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result.Success());
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var addressServiceMock = new Mock<IAddressService>();
        addressServiceMock
            .Setup(m => m.GetOrCreateAddressAsync(editAddress))
            .ReturnsAsync(Result<Address>.Success(updatedAddress));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(c => c.GetCategoriesByNames(It.IsAny<List<string>>()))
            .Returns(Result<List<Category>>.Success(expectedCategories));

        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(name, result.Value!.Name);
        Assert.Equal(description, result.Value!.Description);
        Assert.Equal(startDate, result.Value!.StartDate);
        Assert.Equal(endDate, result.Value!.EndDate);
        Assert.Equal(minimumAge, result.Value!.MinimumAge);
        Assert.Equal(eventStatus, result.Value!.EventStatus);
        Assert.Equal(expectedCategories.Count, result.Value!.Categories.Count);
        Assert.Equal(updatedAddress, result.Value!.Address);
        
        // Verify repository was called to save the updated event
        eventRepositoryMock.Verify(e => e.SaveEventAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task EditEventAsync_WhenEventNotFound_ShouldReturnError()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 6, 1);
        DateTime endDate = new DateTime(2025, 7, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz"),
            new EditEventCategoryDto("classical")
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Failure(StatusCodes.Status404NotFound, "Event not found or not owned by organizer"));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var categoryServiceMock = new Mock<ICategoryService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Event not found or not owned by organizer", result.ErrorMsg);
    }

    [Fact]
    public async Task EditEventAsync_WhenEndDateBeforeStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 7, 1);
        DateTime endDate = new DateTime(2025, 6, 1); // End date before start date
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Organizer = organizer,
            TicketTypes = [],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz")
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("End date should be after start date", result.ErrorMsg);
    }

    [Fact]
    public async Task EditEventAsync_WhenStartDateChangedAndInPast_ShouldReturnBadRequest()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 4, 1); // Start date in the past compared to current date
        DateTime endDate = new DateTime(2025, 5, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Organizer = organizer,
            TicketTypes = [],
            StartDate = new DateTime(2025, 3, 1),
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz")
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15)); // Current date is after start date
        
        var categoryServiceMock = new Mock<ICategoryService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Start date is in the past", result.ErrorMsg);
    }

    [Fact]
    public async Task EditEventAsync_StartDateNotChangedAndInPast_ShouldUpdateEvent()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description of a concert";
        DateTime startDate = new DateTime(2020, 1, 1);
        DateTime endDate = new DateTime(2025, 7, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Description = "Original description",
            StartDate = new DateTime(2020, 1, 1),
            EndDate = new DateTime(2025, 5, 15),
            MinimumAge = 18,
            EventStatus = EventStatus.TicketsAvailable,
            Organizer = organizer,
            Categories = new List<Category>
            {
                new Category { Name = "rock" }
            },
            Address = new Address
            {
                Country = "United States",
                City = "Chicago",
                Street = "State st",
                HouseNumber = 100,
                FlatNumber = null,
                PostalCode = "60001"
            },
            TicketTypes = [],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz"),
            new EditEventCategoryDto("classical")
        ];
        
        List<Category> expectedCategories =
        [
            new Category { Name = "jazz" },
            new Category { Name = "classical" }
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        var updatedAddress = new Address
        {
            Country = "United States",
            City = "New York",
            Street = "Broadway",
            HouseNumber = 42,
            FlatNumber = null,
            PostalCode = "10001"
        };
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        eventRepositoryMock
            .Setup(e => e.SaveEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result.Success());
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var addressServiceMock = new Mock<IAddressService>();
        addressServiceMock
            .Setup(m => m.GetOrCreateAddressAsync(editAddress))
            .ReturnsAsync(Result<Address>.Success(updatedAddress));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(c => c.GetCategoriesByNames(It.IsAny<List<string>>()))
            .Returns(Result<List<Category>>.Success(expectedCategories));

        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(name, result.Value!.Name);
        Assert.Equal(description, result.Value!.Description);
        Assert.Equal(startDate, result.Value!.StartDate);
        Assert.Equal(endDate, result.Value!.EndDate);
        Assert.Equal(minimumAge, result.Value!.MinimumAge);
        Assert.Equal(eventStatus, result.Value!.EventStatus);
        Assert.Equal(expectedCategories.Count, result.Value!.Categories.Count);
        Assert.Equal(updatedAddress, result.Value!.Address);
        
        // Verify repository was called to save the updated event
        eventRepositoryMock.Verify(e => e.SaveEventAsync(It.IsAny<Event>()), Times.Once);
    }
    
    [Fact]
    public async Task EditEventAsync_WhenTicketTypeAvailableFromAfterEndDate_ShouldReturnBadRequest()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 4, 1);
        DateTime endDate = new DateTime(2025, 5, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Organizer = organizer,
            TicketTypes = [
                new TicketType
                {
                    AvailableFrom = new DateTime(3000, 12, 12),
                }
            ],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz")
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        var addressServiceMock = new Mock<IAddressService>();
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2000, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Tickets can't be available after the event is over", result.ErrorMsg);
    }
    
    [Fact]
    public async Task EditEventAsync_WhenAddressServiceFails_ShouldPropagateError()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 6, 1);
        DateTime endDate = new DateTime(2025, 7, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Organizer = organizer,
            TicketTypes = [],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz")
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("", "", "", 0, null, ""); // Invalid address
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var addressServiceMock = new Mock<IAddressService>();
        addressServiceMock
            .Setup(m => m.GetOrCreateAddressAsync(editAddress))
            .ReturnsAsync(Result<Address>.Failure(StatusCodes.Status400BadRequest, "Invalid address data"));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Invalid address data", result.ErrorMsg);
    }

    [Fact]
    public async Task EditEventAsync_WhenCategoryServiceFails_ShouldPropagateError()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 6, 1);
        DateTime endDate = new DateTime(2025, 7, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Organizer = organizer,
            TicketTypes = [],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("non-existent-category")
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        var updatedAddress = new Address
        {
            Country = "United States",
            City = "New York",
            Street = "Broadway",
            HouseNumber = 42,
            FlatNumber = null,
            PostalCode = "10001"
        };
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var addressServiceMock = new Mock<IAddressService>();
        addressServiceMock
            .Setup(m => m.GetOrCreateAddressAsync(editAddress))
            .ReturnsAsync(Result<Address>.Success(updatedAddress));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(c => c.GetCategoriesByNames(It.IsAny<List<string>>()))
            .Returns(Result<List<Category>>.Failure(StatusCodes.Status404NotFound, "One or more categories not found"));
        
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("One or more categories not found", result.ErrorMsg);
    }

    [Fact]
    public async Task EditEventAsync_WhenSaveEventFails_ShouldPropagateError()
    {
        // Arrange
        var organizer = new Organizer
        {
            Email = "organizer@example.com",
            IsVerified = true
        };
        var eventId = Guid.NewGuid();
        string name = "Updated Concert";
        string description = "Updated description";
        DateTime startDate = new DateTime(2025, 6, 1);
        DateTime endDate = new DateTime(2025, 7, 1);
        uint? minimumAge = 21;
        EventStatus eventStatus = EventStatus.SoldOut;
        
        var existingEvent = new Event
        {
            Id = eventId,
            Name = "Original Concert",
            Organizer = organizer,
            TicketTypes = [],
        };
        
        List<EditEventCategoryDto> categories =
        [
            new EditEventCategoryDto("jazz")
        ];
        
        List<Category> expectedCategories =
        [
            new Category { Name = "jazz" }
        ];
        
        CreateAddressDto editAddress = new CreateAddressDto("United States", "New York", "Broadway", 42, null, "10001");
        var updatedAddress = new Address
        {
            Country = "United States",
            City = "New York",
            Street = "Broadway",
            HouseNumber = 42,
            FlatNumber = null,
            PostalCode = "10001"
        };
        
        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(e => e.GetEventByIdAndOrganizerAsync(eventId, organizer))
            .ReturnsAsync(Result<Event>.Success(existingEvent));
        eventRepositoryMock
            .Setup(e => e.SaveEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result.Failure(StatusCodes.Status500InternalServerError, "Database error occurred"));
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var addressServiceMock = new Mock<IAddressService>();
        addressServiceMock
            .Setup(m => m.GetOrCreateAddressAsync(editAddress))
            .ReturnsAsync(Result<Address>.Success(updatedAddress));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(2025, 4, 15));
        
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(c => c.GetCategoriesByNames(It.IsAny<List<string>>()))
            .Returns(Result<List<Category>>.Success(expectedCategories));
        
        var paginationServiceMock = new Mock<IPaginationService>();
        var ticketServiceMock = new Mock<ITicketService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var mailServiceMock = new Mock<IMailService>();
        
        var blobServiceMock = new Mock<IBlobService>();
        
        var sut = new EventService(eventRepositoryMock.Object, organizerServiceMock.Object, addressServiceMock.Object, dateTimeServiceMock.Object, paginationServiceMock.Object, categoryServiceMock.Object, ticketServiceMock.Object, customerRepositoryMock.Object, mailServiceMock.Object, blobServiceMock.Object);        
        // Act
        var result = await sut.EditEventAsync(
            organizer, 
            eventId, 
            name, 
            description, 
            startDate, 
            endDate, 
            minimumAge, 
            editAddress, 
            categories, 
            eventStatus);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("Database error occurred", result.ErrorMsg);
    }
}