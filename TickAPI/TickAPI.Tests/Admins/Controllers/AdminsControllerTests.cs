using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Admins.Abstractions;
using TickAPI.Admins.Controllers;
using TickAPI.Admins.DTOs.Request;
using TickAPI.Admins.DTOs.Response;
using TickAPI.Admins.Models;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.DTOs.Request;
using Xunit;

namespace TickAPI.Tests.Admins.Controllers;

public class AdminsControllerTests
{
    private readonly Mock<IGoogleAuthService> _mockGoogleAuthService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IAdminService> _mockAdminService;
    private readonly Mock<IClaimsService> _mockClaimsService;
    private readonly AdminsController _controller;

    public AdminsControllerTests()
    {
        _mockGoogleAuthService = new Mock<IGoogleAuthService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockAdminService = new Mock<IAdminService>();
        _mockClaimsService = new Mock<IClaimsService>();

        _controller = new AdminsController(
            _mockGoogleAuthService.Object,
            _mockJwtService.Object,
            _mockAdminService.Object,
            _mockClaimsService.Object
        );
    }

    [Fact]
    public async Task GoogleLogin_WithValidAccessToken_ReturnsJwtToken()
    {
        // Arrange
        const string email = "existing@test.com";
        const string accessToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";

        _mockGoogleAuthService
            .Setup(x => x.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email, "First", "Last")));

        _mockAdminService
            .Setup(x => x.GetAdminByEmailAsync(email))
            .ReturnsAsync(Result<Admin>.Success(new Admin{Email = email}));

        _mockJwtService
            .Setup(x => x.GenerateJwtToken(email, UserRole.Admin))
            .Returns(Result<string>.Success(jwtToken));

        // Act
        var result = await _controller.GoogleLogin(new GoogleAdminLoginDto(accessToken));

        // Assert
        var actionResult = Assert.IsType<ActionResult<GoogleAdminLoginResponseDto>>(result);
        var okResult = Assert.IsType<GoogleAdminLoginResponseDto>(actionResult.Value);
        Assert.Equal(jwtToken, okResult.Token);
    }

    [Fact]
    public async Task GoogleLogin_WithInvalidAccessToken_ReturnsErrorStatusCode()
    {
        // Arrange
        const string accessToken = "valid-google-token";

        _mockGoogleAuthService
            .Setup(x => x.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Failure(StatusCodes.Status401Unauthorized, "Invalid token"));

        // Act
        var result = await _controller.GoogleLogin(new  GoogleAdminLoginDto(accessToken));

        // Assert
        var actionResult = Assert.IsType<ActionResult<GoogleAdminLoginResponseDto>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
        Assert.Equal("Invalid token", objectResult.Value);
    }

    [Fact]
    public async Task GoogleLogin_WithNonAdminEmail_ReturnsErrorStatusCode()
    {
        // Arrange
        const string email = "nonadmin@test.com";
        const string accessToken = "valid-google-token";

        _mockGoogleAuthService
            .Setup(x => x.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email,  "First", "Last")));

        _mockAdminService
            .Setup(x => x.GetAdminByEmailAsync(email))
            .ReturnsAsync(Result<Admin>.Failure(StatusCodes.Status404NotFound, "User is not an admin"));

        // Act
        var result = await _controller.GoogleLogin(new GoogleAdminLoginDto(accessToken));

        // Assert
        var actionResult = Assert.IsType<ActionResult<GoogleAdminLoginResponseDto>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        Assert.Equal("User is not an admin", objectResult.Value);
    }
    

    [Fact]
    public async Task AboutMe_WithValidClaims_ReturnsAdminData()
    {
        // Arrange
        var email = "admin@example.com";
        var admin = new Admin { Email = email, Login = "admin" };
        
        // Mock the HttpContext and User
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockClaimsService
            .Setup(x => x.GetEmailFromClaims(It.IsAny<IEnumerable<Claim>>()))
            .Returns(Result<string>.Success(email));

        _mockAdminService
            .Setup(x => x.GetAdminByEmailAsync(email))
            .ReturnsAsync(Result<Admin>.Success(admin));

        // Act
        var result = await _controller.AboutMe();

        // Assert
        var actionResult = Assert.IsType<ActionResult<AboutMeAdminResponseDto>>(result);
        var okResult = Assert.IsType<AboutMeAdminResponseDto>(actionResult.Value);
        Assert.Equal(admin.Email, okResult.Email);
        Assert.Equal(admin.Login, okResult.Login);
    }

    [Fact]
    public async Task AboutMe_WithInvalidClaims_ReturnsErrorStatusCode()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockClaimsService
            .Setup(x => x.GetEmailFromClaims(It.IsAny<IEnumerable<Claim>>()))
            .Returns(Result<string>.Failure(StatusCodes.Status401Unauthorized, "Email claim not found"));

        // Act
        var result = await _controller.AboutMe();

        // Assert
        var actionResult = Assert.IsType<ActionResult<AboutMeAdminResponseDto>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
        Assert.Equal("Email claim not found", objectResult.Value);
    }
    
}
