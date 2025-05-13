using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.DTOs.Response;
using TickAPI.Organizers.Models;
using TickAPI.Organizers.Services;

namespace TickAPI.Tests.Organizers.Services;

public class OrganizerServiceTests
{
    [Fact]
    public async Task GetOrganizerByEmailAsync_WhenOrganizerWithEmailIsReturnedFromRepository_ShouldReturnOrganizer()
    {
        // Arrange
        const string email = "example@test.com";

        var organizer = new Organizer
        {
            Email = email
        };
        
        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.GetOrganizerByEmailAsync(email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(organizer, result.Value);
    }

    [Fact]
    public async Task GetOrganizerByEmailAsync_WhenOrganizerWithEmailIsNotReturnedFromRepository_ShouldReturnFailure()
    {
        // Arrange
        const string email = "example@test.com";

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Failure(StatusCodes.Status404NotFound, $"organizer with email '{email}' not found"));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.GetOrganizerByEmailAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal($"organizer with email '{email}' not found", result.ErrorMsg);
    }

    [Fact]
    public async Task CreateNewOrganizerAsync_WhenOrganizerDataIsValid_ShouldReturnNewOrganizer()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        const string email = "example@test.com";
        const string firstName = "First";
        const string lastName = "Last";
        const string displayName = "Display";
        DateTime currentDate = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Failure(StatusCodes.Status404NotFound, $"organizer with email '{email}' not found"));
        organizerRepositoryMock
            .Setup(m => m.AddNewOrganizerAsync(It.IsAny<Organizer>()))
            .Callback<Organizer>(o => o.Id = id)
            .Returns(Task.CompletedTask);
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(currentDate);
        
        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.CreateNewOrganizerAsync(email, firstName, lastName, displayName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal(email, result.Value!.Email);
        Assert.Equal(firstName, result.Value!.FirstName);
        Assert.Equal(lastName, result.Value!.LastName);
        Assert.Equal(displayName, result.Value!.DisplayName);
        Assert.False(result.Value!.IsVerified);
        Assert.Equal(currentDate, result.Value!.CreationDate);
    }
    
    [Fact]
    public async Task CreateNewOrganizerAsync_WhenLastNameIsNull_ShouldReturnNewOrganizer()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        const string email = "example@test.com";
        const string firstName = "First";
        const string lastName = null;
        const string displayName = "Display";
        DateTime currentDate = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Failure(StatusCodes.Status404NotFound, $"organizer with email '{email}' not found"));
        organizerRepositoryMock
            .Setup(m => m.AddNewOrganizerAsync(It.IsAny<Organizer>()))
            .Callback<Organizer>(o => o.Id = id)
            .Returns(Task.CompletedTask);
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock
            .Setup(m => m.GetCurrentDateTime())
            .Returns(currentDate);
        
        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.CreateNewOrganizerAsync(email, firstName, lastName, displayName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal(email, result.Value!.Email);
        Assert.Equal(firstName, result.Value!.FirstName);
        Assert.Equal(lastName, result.Value!.LastName);
        Assert.Equal(displayName, result.Value!.DisplayName);
        Assert.False(result.Value!.IsVerified);
        Assert.Equal(currentDate, result.Value!.CreationDate);
    }

    [Fact]
    public async Task CreateNewOrganizerAsync_WhenWithNotUniqueEmail_ShouldReturnFailure()
    {
        // Arrange
        const string email = "example@test.com";
        const string firstName = "First";
        const string lastName = "Last";
        const string displayName = "Display";

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = email }));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        
        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.CreateNewOrganizerAsync(email, firstName, lastName, displayName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal($"organizer with email '{email}' already exists", result.ErrorMsg);
    }

    [Fact]
    public async Task VerifyOrganizerByEmailAsync_WhenVerificationSuccessful_ShouldReturnSuccess()
    {
        const string email = "example@test.com";
        
        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.VerifyOrganizerByEmailAsync(email))
            .ReturnsAsync(Result.Success());
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        
        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.VerifyOrganizerByEmailAsync(email);

        // Assert
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task VerifyOrganizerByEmailAsync_WhenVerificationNotSuccessful_ShouldReturnFailure()
    {
        const string email = "example@test.com";
        
        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.VerifyOrganizerByEmailAsync(email))
            .ReturnsAsync(Result.Failure(StatusCodes.Status404NotFound, $"organizer with email '{email}' not found"));
        
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        
        var paginationServiceMock = new Mock<IPaginationService>();
                     
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.VerifyOrganizerByEmailAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal($"organizer with email '{email}' not found", result.ErrorMsg);
    }
    
    [Fact]
    public async Task GetUnverifiedOrganizersAsync_WhenPaginationSuccessful_ShouldReturnPaginatedUnverifiedOrganizers()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        var unverifiedOrganizers = new List<Organizer>
        {
            new() { Email = "unverified1@test.com", FirstName = "First1", LastName = "Last1", DisplayName = "Display1", IsVerified = false },
            new() { Email = "unverified2@test.com", FirstName = "First2", LastName = "Last2", DisplayName = "Display2", IsVerified = false },
            new() { Email = "unverified3@test.com", FirstName = "First3", LastName = "Last3", DisplayName = "Display3", IsVerified = false }
        }.AsQueryable();

        var paginationDetails = new PaginationDetails(0, 3);
        var paginatedData = new PaginatedData<Organizer>(
            unverifiedOrganizers.ToList(),
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var expectedDtos = new List<GetUnverifiedOrganizerResponseDto>
        {
            new("unverified1@test.com", "First1", "Last1", "Display1"),
            new("unverified2@test.com", "First2", "Last2", "Display2"),
            new("unverified3@test.com", "First3", "Last3", "Display3")
        };

        var mappedData = new PaginatedData<GetUnverifiedOrganizerResponseDto>(
            expectedDtos,
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizers())
            .Returns(unverifiedOrganizers);

        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock
            .Setup(m => m.PaginateAsync(unverifiedOrganizers, pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Organizer>>.Success(paginatedData));
        
        // Capture and verify the mapping function
        Func<Organizer, GetUnverifiedOrganizerResponseDto> capturedMapFunction = null;
        paginationServiceMock
            .Setup(m => m.MapData(paginatedData, It.IsAny<Func<Organizer, GetUnverifiedOrganizerResponseDto>>()))
            .Returns<PaginatedData<Organizer>, Func<Organizer, GetUnverifiedOrganizerResponseDto>>((source, mapFunc) =>
            {
                capturedMapFunction = mapFunc;
                return mappedData;
            });

        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.GetUnverifiedOrganizersAsync(page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mappedData, result.Value);
        Assert.Equal(3, result.Value!.Data.Count);
        
        // Verify each DTO was correctly mapped
        for (int i = 0; i < expectedDtos.Count; i++)
        {
            Assert.Equal(expectedDtos[i], result.Value.Data[i]);
        }
        
        // Verify the mapping function works correctly
        Assert.NotNull(capturedMapFunction);
        var testOrganizer = new Organizer { Email = "test@example.com", FirstName = "TestFirst", LastName = "TestLast", DisplayName = "TestDisplay" };
        var mappedDto = capturedMapFunction(testOrganizer);
        var expectedDto = new GetUnverifiedOrganizerResponseDto("test@example.com", "TestFirst", "TestLast", "TestDisplay");
        Assert.Equal(expectedDto, mappedDto);
    }

    [Fact]
    public async Task GetUnverifiedOrganizersAsync_WhenFilteringOrganizers_ShouldOnlyReturnUnverifiedOnes()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        var mixedOrganizers = new List<Organizer>
        {
            new() { Email = "unverified1@test.com", FirstName = "First1", LastName = "Last1", DisplayName = "Display1", IsVerified = false },
            new() { Email = "verified1@test.com", FirstName = "First2", LastName = "Last2", DisplayName = "Display2", IsVerified = true },
            new() { Email = "unverified2@test.com", FirstName = "First3", LastName = "Last3", DisplayName = "Display3", IsVerified = false }
        }.AsQueryable();

        var filteredOrganizers = mixedOrganizers.Where(o => !o.IsVerified).ToList();

        var paginationDetails = new PaginationDetails(0, 2);
        var paginatedData = new PaginatedData<Organizer>(
            filteredOrganizers,
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var expectedDtos = new List<GetUnverifiedOrganizerResponseDto>
        {
            new("unverified1@test.com", "First1", "Last1", "Display1"),
            new("unverified2@test.com", "First3", "Last3", "Display3")
        };

        var mappedData = new PaginatedData<GetUnverifiedOrganizerResponseDto>(
            expectedDtos,
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizers())
            .Returns(mixedOrganizers);

        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock
            .Setup(m => m.PaginateAsync(It.Is<IQueryable<Organizer>>(q => q.Count() == 2), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Organizer>>.Success(paginatedData));
        paginationServiceMock
            .Setup(m => m.MapData(paginatedData, It.IsAny<Func<Organizer, GetUnverifiedOrganizerResponseDto>>()))
            .Returns(mappedData);

        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.GetUnverifiedOrganizersAsync(page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mappedData, result.Value);
        Assert.Equal(2, result.Value!.Data.Count);
        Assert.Equal(expectedDtos, result.Value.Data);
        Assert.DoesNotContain(result.Value.Data, dto => dto.Email == "verified1@test.com");
    }

    [Fact]
    public async Task GetUnverifiedOrganizersAsync_WhenNoPaginationResults_ShouldReturnEmptyList()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        var emptyOrganizers = new List<Organizer>().AsQueryable();

        var paginationDetails = new PaginationDetails(0, 0);
        var paginatedData = new PaginatedData<Organizer>(
            new List<Organizer>(),
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var mappedData = new PaginatedData<GetUnverifiedOrganizerResponseDto>(
            new List<GetUnverifiedOrganizerResponseDto>(),
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizers())
            .Returns(emptyOrganizers);

        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock
            .Setup(m => m.PaginateAsync(It.IsAny<IQueryable<Organizer>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Organizer>>.Success(paginatedData));
        paginationServiceMock
            .Setup(m => m.MapData(paginatedData, It.IsAny<Func<Organizer, GetUnverifiedOrganizerResponseDto>>()))
            .Returns(mappedData);

        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.GetUnverifiedOrganizersAsync(page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mappedData, result.Value);
        Assert.Empty(result.Value!.Data);
    }

    [Fact]
    public async Task GetUnverifiedOrganizersAsync_WithNullLastNames_ShouldMapCorrectly()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        var unverifiedOrganizers = new List<Organizer>
        {
            new() { Email = "nulllast@test.com", FirstName = "First", LastName = null, DisplayName = "Display", IsVerified = false },
        }.AsQueryable();

        var paginationDetails = new PaginationDetails(0, 1);
        var paginatedData = new PaginatedData<Organizer>(
            unverifiedOrganizers.ToList(),
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var expectedDto = new GetUnverifiedOrganizerResponseDto("nulllast@test.com", "First", null, "Display");
        var mappedData = new PaginatedData<GetUnverifiedOrganizerResponseDto>(
            new List<GetUnverifiedOrganizerResponseDto> { expectedDto },
            page,
            pageSize,
            false,
            false,
            paginationDetails
        );

        var organizerRepositoryMock = new Mock<IOrganizerRepository>();
        organizerRepositoryMock
            .Setup(m => m.GetOrganizers())
            .Returns(unverifiedOrganizers);

        var dateTimeServiceMock = new Mock<IDateTimeService>();

        var paginationServiceMock = new Mock<IPaginationService>();
        paginationServiceMock
            .Setup(m => m.PaginateAsync(It.IsAny<IQueryable<Organizer>>(), pageSize, page))
            .ReturnsAsync(Result<PaginatedData<Organizer>>.Success(paginatedData));
        
        // Verify the mapping function handles null LastName correctly
        Func<Organizer, GetUnverifiedOrganizerResponseDto> capturedMapFunction = null;
        paginationServiceMock
            .Setup(m => m.MapData(paginatedData, It.IsAny<Func<Organizer, GetUnverifiedOrganizerResponseDto>>()))
            .Returns<PaginatedData<Organizer>, Func<Organizer, GetUnverifiedOrganizerResponseDto>>((source, mapFunc) =>
            {
                capturedMapFunction = mapFunc;
                return mappedData;
            });

        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object,
            paginationServiceMock.Object
        );

        // Act
        var result = await sut.GetUnverifiedOrganizersAsync(page, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mappedData, result.Value);
        Assert.Single(result.Value!.Data);
        Assert.Equal(expectedDto, result.Value.Data[0]);
        Assert.Null(result.Value.Data[0].LastName);
        
        // Verify the mapping function works correctly with null LastName
        Assert.NotNull(capturedMapFunction);
        var testOrganizer = new Organizer { Email = "test@example.com", FirstName = "TestFirst", LastName = null, DisplayName = "TestDisplay" };
        var mappedDto = capturedMapFunction(testOrganizer);
        var expectedMappedDto = new GetUnverifiedOrganizerResponseDto("test@example.com", "TestFirst", null, "TestDisplay");
        Assert.Equal(expectedMappedDto, mappedDto);
    }
}