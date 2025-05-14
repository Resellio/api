using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Controllers;
using TickAPI.Organizers.DTOs.Request;
using TickAPI.Organizers.DTOs.Response;
using TickAPI.Organizers.Models;

namespace TickAPI.Tests.Organizers.Controllers;

public class OrganizersControllerTests
{
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndVerifiedOrganizerExists_ShouldReturnValidVerifiedLoginDto()
    {
        // Arrange
        const string email = "existing@test.com";
        const string accessToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock
            .Setup(m => m.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email, "First", "Last")));
    
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = email, IsVerified = true }));
    
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock
            .Setup(m => m.GenerateJwtToken(email, UserRole.Organizer))
            .Returns(Result<string>.Success(jwtToken));
        
        var claimsServiceMock = new Mock<IClaimsService>();
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object,
            jwtServiceMock.Object,
            organizerServiceMock.Object,
            claimsServiceMock.Object
        );
        
        // Act
        var actionResult = await sut.GoogleLogin(new GoogleOrganizerLoginDto(accessToken));
        
        // Assert
        Assert.Equal(jwtToken, actionResult.Value!.Token);
        Assert.False(actionResult.Value!.IsNewOrganizer);
        Assert.True(actionResult.Value!.IsVerified);
    }
    
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndUnverifiedOrganizerExists_ShouldReturnValidUnverifiedLoginDto()
    {
        // Arrange
        const string email = "unverified@test.com";
        const string accessToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock
            .Setup(m => m.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email, "First", "Last")));
    
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(new Organizer { Email = email, IsVerified = false }));
    
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock
            .Setup(m => m.GenerateJwtToken(email, UserRole.UnverifiedOrganizer))
            .Returns(Result<string>.Success(jwtToken));
        
        var claimsServiceMock = new Mock<IClaimsService>();
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object,
            jwtServiceMock.Object,
            organizerServiceMock.Object,
            claimsServiceMock.Object
        );
        
        // Act
        var actionResult = await sut.GoogleLogin(new GoogleOrganizerLoginDto(accessToken));
        
        // Assert
        Assert.Equal(jwtToken, actionResult.Value!.Token);
        Assert.False(actionResult.Value!.IsNewOrganizer);
        Assert.False(actionResult.Value!.IsVerified);
    }

    [Fact]
    public async Task
        GoogleLogin_WhenAuthSuccessAndOrganizerDoesNotExist_ShouldCreateValidNewOrganizerLoginDto()
    {
        // Arrange
        const string email = "new@test.com";
        const string accessToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock
            .Setup(m => m.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email, "First", "Last")));
    
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Failure(StatusCodes.Status404NotFound, $"organizer with email '{email}' not found"));
    
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock
            .Setup(m => m.GenerateJwtToken(email, UserRole.NewOrganizer))
            .Returns(Result<string>.Success(jwtToken));
        
        var claimsServiceMock = new Mock<IClaimsService>();
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object,
            jwtServiceMock.Object,
            organizerServiceMock.Object,
            claimsServiceMock.Object
        );
        
        // Act
        var actionResult = await sut.GoogleLogin(new GoogleOrganizerLoginDto(accessToken));
        
        // Assert
        Assert.Equal(jwtToken, actionResult.Value!.Token);
        Assert.True(actionResult.Value!.IsNewOrganizer);
        Assert.False(actionResult.Value!.IsVerified);
    }

    [Fact]
    public async Task CreateOrganizer_WhenCreatingAccountIsSuccessful_ShouldReturnToken()
    {
        // Arrange
        const string email = "new@test.com";
        const string firstName = "First";
        const string lastName = "Last";
        const string displayName = "Display";
        const string jwtToken = "valid-jwt-token";
        
        var organizer = new Organizer
        {
            Id = Guid.NewGuid(),
            Email = email, 
            FirstName = firstName, 
            LastName = lastName,
            DisplayName = displayName,
            IsVerified = false
        };
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.CreateNewOrganizerAsync(email, firstName, lastName, displayName))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(m => m.GenerateJwtToken(email, UserRole.UnverifiedOrganizer))
            .Returns(Result<string>.Success(jwtToken));

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
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            organizerServiceMock.Object,
            claimsServiceMock.Object);


        sut.ControllerContext = controllerContext;
        
        // Act
        var actionResult = await sut.CreateOrganizer(new CreateOrganizerDto(firstName, lastName, displayName));
        
        // Assert
        Assert.Equal(jwtToken, actionResult.Value!.Token);
    }
    
    [Fact]
    public async Task CreateOrganizer_WhenMissingEmailClaim_ShouldReturnBadRequest()
    {
        // Arrange
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var jwtServiceMock = new Mock<IJwtService>();

        var claimsServiceMock = new Mock<IClaimsService>();
        claimsServiceMock.Setup(m => m.GetEmailFromClaims(It.IsAny<IEnumerable<Claim>>())).Returns(Result<string>.Failure(StatusCodes.Status400BadRequest, "missing email claim"));
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            organizerServiceMock.Object,
            claimsServiceMock.Object);
        
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()))
            }
        };
        
        // Act
        var actionResult = await sut.CreateOrganizer(new CreateOrganizerDto("First", "Last", "Display"));
        
        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("missing email claim", objectResult.Value);
    }

    [Fact]
    public async Task VerifyOrganizer_WhenVerificationSuccessful_ShouldReturnOk()
    {
        // Arrange
        const string email = "new@test.com";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.VerifyOrganizerByEmailAsync(email))
            .ReturnsAsync(Result.Success());
        
        var jwtServiceMock = new Mock<IJwtService>();

        var claimsServiceMock = new Mock<IClaimsService>();
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            organizerServiceMock.Object,
            claimsServiceMock.Object);
        
        // Act
        var actionResult = await sut.VerifyOrganizer(new VerifyOrganizerDto(email));
        
        // Assert
        var result = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task AboutMe_WithValidEmailClaim_ShouldReturnOrganizerDetails()
    {
        // Arrange
        const string email = "example@test.com";
        const string firstName = "First";
        const string lastName = "Last";
        const string displayName = "Display";
        const bool isVerified = true;
        DateTime creationDate = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        
        var organizer = new Organizer
        {
            Id = Guid.NewGuid(),
            Email = email, 
            FirstName = firstName, 
            LastName = lastName,
            DisplayName = displayName,
            IsVerified = isVerified,
            CreationDate = creationDate
        };
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Success(organizer));
        
        var jwtServiceMock = new Mock<IJwtService>();

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
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            organizerServiceMock.Object,
            claimsServiceMock.Object);

        sut.ControllerContext = controllerContext;
        
        // Act
        var actionResult = await sut.AboutMe();
        
        // Assert
        Assert.Equal(email, actionResult.Value?.Email);
        Assert.Equal(firstName, actionResult.Value?.FirstName);
        Assert.Equal(lastName, actionResult.Value?.LastName);
        Assert.Equal(displayName, actionResult.Value?.DisplayName);
        Assert.Equal(isVerified, actionResult.Value?.IsVerified);
        Assert.Equal(creationDate, actionResult.Value?.CreationDate);
    }
    
    [Fact]
    public async Task AboutMe_WithMissingEmailClaim_ShouldReturnBadRequest()
    {
        // Arrange
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        
        var jwtServiceMock = new Mock<IJwtService>();

        var claimsServiceMock = new Mock<IClaimsService>();
        claimsServiceMock.Setup(m => m.GetEmailFromClaims(It.IsAny<IEnumerable<Claim>>())).Returns(Result<string>.Failure(StatusCodes.Status400BadRequest, "missing email claim"));
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            organizerServiceMock.Object,
            claimsServiceMock.Object);
        
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()))
            }
        };
        
        // Act
        var actionResult = await sut.AboutMe();
        
        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("missing email claim", objectResult.Value);
    }

    [Fact]
    public async Task AboutMe_WhenOrganizerNotFound_ShouldReturnInternalServerError()
    {
        // Arrange
        const string email = "example@test.com";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        
        var organizerServiceMock = new Mock<IOrganizerService>();
        organizerServiceMock
            .Setup(m => m.GetOrganizerByEmailAsync(email))
            .ReturnsAsync(Result<Organizer>.Failure(StatusCodes.Status404NotFound, $"organizer with email '{email}' not found"));
        
        var jwtServiceMock = new Mock<IJwtService>();

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
        
        var sut = new OrganizersController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            organizerServiceMock.Object,
            claimsServiceMock.Object);
        
        sut.ControllerContext = controllerContext;
        
        // Act
        var actionResult = await sut.AboutMe();
        
        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("cannot find organizer in database for authorized organizer request", objectResult.Value);
    }
}