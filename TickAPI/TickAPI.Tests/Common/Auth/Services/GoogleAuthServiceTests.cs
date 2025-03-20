using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text.Json;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Auth.Services;

namespace TickAPI.Tests.Common.Auth.Services;

public class GoogleAuthServiceTests
{
    private readonly Mock<IGoogleDataFetcher> _googleDataFetcherMock;
    
    public GoogleAuthServiceTests()
    {
        var validMessage = new HttpResponseMessage(HttpStatusCode.OK);
        validMessage.Content = new StringContent(JsonSerializer.Serialize(new GoogleUserData("example@test.com", "Name", "Surname")));
        
        var unauthorizedMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        
        var wrongContentMessage = new HttpResponseMessage(HttpStatusCode.OK);
        wrongContentMessage.Content = new StringContent("null");
        
        _googleDataFetcherMock = new Mock<IGoogleDataFetcher>();
        _googleDataFetcherMock
            .Setup(m => m.FetchUserDataAsync("validToken"))
            .ReturnsAsync(validMessage);
        _googleDataFetcherMock
            .Setup(m => m.FetchUserDataAsync("invalidToken"))
            .ReturnsAsync(unauthorizedMessage);
        _googleDataFetcherMock
            .Setup(m => m.FetchUserDataAsync("nullToken"))
            .ReturnsAsync(wrongContentMessage);
        _googleDataFetcherMock
            .Setup(m => m.FetchUserDataAsync("throwToken"))
            .ThrowsAsync(new Exception("An exception occured"));
    }
    
    [Fact]
    public async Task GetUserDataFromAccessToken_WhenDataFetcherReturnsValidResponse_ShouldReturnUserDataFromResponse()
    {
        var sut = new GoogleAuthService(_googleDataFetcherMock.Object);
        
        var result = await sut.GetUserDataFromAccessToken("validToken");
        
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("example@test.com", result.Value!.Email);
        Assert.Equal("Name", result.Value!.GivenName);
        Assert.Equal("Surname", result.Value!.FamilyName);
    }

    [Fact]
    public async Task
        GetUserDataFromAccessToken_WhenDataFetcherReturnsResponseWithErrorStatusCode_ShouldReturnFailure()
    {
        var sut = new GoogleAuthService(_googleDataFetcherMock.Object);
        
        var result = await sut.GetUserDataFromAccessToken("invalidToken");
        
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Invalid Google access token", result.ErrorMsg);
    }

    [Fact]
    public async Task GetUserDataFromAccessToken_WhenDataFetcherReturnsNullResponse_ShouldReturnFailure()
    {
        var sut = new GoogleAuthService(_googleDataFetcherMock.Object);
        
        var result = await sut.GetUserDataFromAccessToken("nullToken");
        
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("Failed to parse Google user info", result.ErrorMsg);
    }

    [Fact]
    public async Task GetUserDataFromAccessToken_WhenDataFetcherThrowsAnException_ShouldReturnFailure()
    {
        var sut = new GoogleAuthService(_googleDataFetcherMock.Object);
        
        var result = await sut.GetUserDataFromAccessToken("throwToken");
        
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal($"Error fetching user data: An exception occured", result.ErrorMsg);
    }
}