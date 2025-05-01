using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Admins.Abstractions;
using TickAPI.Admins.Models;
using TickAPI.Admins.Services;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;
using TickAPI.Customers.Services;

namespace TickAPI.Tests.Admins.Services;

public class AdminsServiceTests
{
    [Fact]
    public async Task GetAdminByEmailAsync_WhenAdminWithEmailIsReturnedFromRepository_ShouldReturnUser()
    {
        var admin = new Admin
        {
            Email = "example@test.com"
        };
        var adminRepositoryMock = new Mock<IAdminRepository>();
        adminRepositoryMock.Setup(m => m.GetAdminByEmailAsync(admin.Email)).ReturnsAsync(Result<Admin>.Success(admin));
        var sut = new AdminService(adminRepositoryMock.Object);

        var result = await sut.GetAdminByEmailAsync(admin.Email);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(admin, result.Value);
    }

    [Fact]
    public async Task GetAdminByEmailAsync_WhenAdminWithEmailIsNotReturnedFromRepository_ShouldReturnFailure()
    {
        const string email = "not@existing.com";
        var adminRepositoryMock = new Mock<IAdminRepository>();
        adminRepositoryMock.Setup(m => m.GetAdminByEmailAsync(email)).ReturnsAsync(Result<Admin>.Failure(StatusCodes.Status404NotFound, $"admin with email '{email}' not found"));
        var sut = new AdminService(adminRepositoryMock.Object);

        var result = await sut.GetAdminByEmailAsync(email);

        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal($"admin with email '{email}' not found", result.ErrorMsg);
    }

}