using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;
using TickAPI.Customers.Services;

namespace TickAPI.Tests.Customers.Services;

public class CustomerServiceTests
{
    [Fact]
    public async Task GetCustomerByEmailAsync_WhenCustomerWithEmailIsReturnedFromRepository_ShouldReturnUser()
    {
        var customer = new Customer
        {
            Email = "example@test.com"
        };
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(m => m.GetCustomerByEmailAsync(customer.Email)).ReturnsAsync(Result<Customer>.Success(customer));
        var sut = new CustomerService(customerRepositoryMock.Object, dateTimeServiceMock.Object);

        var result = await sut.GetCustomerByEmailAsync(customer.Email);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(customer, result.Value);
    }

    [Fact]
    public async Task GetCustomerByEmailAsync_WhenCustomerWithEmailIsNotReturnedFromRepository_ShouldReturnFailure()
    {
        const string email = "not@existing.com";
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(m => m.GetCustomerByEmailAsync(email)).ReturnsAsync(Result<Customer>.Failure(StatusCodes.Status404NotFound, $"customer with email '{email}' not found"));
        var sut = new CustomerService(customerRepositoryMock.Object, dateTimeServiceMock.Object);

        var result = await sut.GetCustomerByEmailAsync(email);

        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal($"customer with email '{email}' not found", result.ErrorMsg);
    }

    [Fact]
    public async Task CreateNewCustomerAsync_WhenCustomerWithUniqueEmail_ShouldReturnNewCustomer()
    {
        const string email = "new@customer.com";
        const string firstName = "First";
        const string lastName = "Last";
        Guid id = Guid.NewGuid();
        DateTime createdAt = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(m => m.GetCurrentDateTime()).Returns(createdAt);
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(m => m.GetCustomerByEmailAsync(email)).ReturnsAsync(Result<Customer>.Failure(StatusCodes.Status404NotFound, $"customer with email '{email}' not found"));
        customerRepositoryMock
            .Setup(m => m.AddNewCustomerAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => c.Id = id)
            .Returns(Task.CompletedTask);
        var sut = new CustomerService(customerRepositoryMock.Object, dateTimeServiceMock.Object);

        var result = await sut.CreateNewCustomerAsync(email, firstName, lastName);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value!.Email);
        Assert.Equal(firstName, result.Value!.FirstName);
        Assert.Equal(lastName, result.Value!.LastName);
        Assert.Equal(createdAt, result.Value!.CreationDate);
        Assert.Equal(id, result.Value!.Id);
    }
    
    [Fact]
    public async Task CreateNewCustomerAsync_WhenLastNameIsNull_ShouldReturnNewCustomer()
    {
        const string email = "new@customer.com";
        const string firstName = "First";
        const string lastName = null;
        Guid id = Guid.NewGuid();
        DateTime createdAt = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        dateTimeServiceMock.Setup(m => m.GetCurrentDateTime()).Returns(createdAt);
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(m => m.GetCustomerByEmailAsync(email)).ReturnsAsync(Result<Customer>.Failure(StatusCodes.Status404NotFound, $"customer with email '{email}' not found"));
        customerRepositoryMock
            .Setup(m => m.AddNewCustomerAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => c.Id = id)
            .Returns(Task.CompletedTask);
        var sut = new CustomerService(customerRepositoryMock.Object, dateTimeServiceMock.Object);

        var result = await sut.CreateNewCustomerAsync(email, firstName, lastName);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value!.Email);
        Assert.Equal(firstName, result.Value!.FirstName);
        Assert.Equal(lastName, result.Value!.LastName);
        Assert.Equal(createdAt, result.Value!.CreationDate);
        Assert.Equal(id, result.Value!.Id);
    }

    [Fact]
    public async Task CreateNewCustomerAsync_WhenCustomerWithNotUniqueEmail_ShouldReturnFailure()
    {
        var customer = new Customer
        {
            Email = "already@exists.com"
        };
        var dateTimeServiceMock = new Mock<IDateTimeService>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(m => m.GetCustomerByEmailAsync(customer.Email)).ReturnsAsync(Result<Customer>.Success(customer));
        var sut = new CustomerService(customerRepositoryMock.Object, dateTimeServiceMock.Object);

        var result = await sut.CreateNewCustomerAsync(customer.Email, "First", "Last");
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal($"customer with email '{customer.Email}' already exists", result.ErrorMsg);
    }
}