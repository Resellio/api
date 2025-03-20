using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Organizers.Abstractions;
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

        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object
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

        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object
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
        
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object
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
        
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object
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
        
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object
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
        
        var sut = new OrganizerService(
            organizerRepositoryMock.Object,
            dateTimeServiceMock.Object
        );

        // Act
        var result = await sut.VerifyOrganizerByEmailAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal($"organizer with email '{email}' not found", result.ErrorMsg);
    }
}