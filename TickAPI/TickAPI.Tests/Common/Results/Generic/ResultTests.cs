using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Tests.Common.Results.Generic;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnResultWithValue()
    {
        // Arrange
        const int value = 123;
        
        // Act
        var result = Result<int>.Success(value);
        
        // Assert
        Assert.Equal(value, result.Value);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal("", result.ErrorMsg);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void Failure_ShouldReturnResultWithError()
    {
        // Arrange
        const int statusCode = 500;
        const string errorMsg = "example error msg";
        
        // Act
        var result = Result<int>.Failure(500, errorMsg);
        
        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Fact]
    public void PropagateError_WhenResultWithErrorPassed_ShouldReturnResultWithError()
    {
        // Arrange
        const int statusCode = 500;
        const string errorMsg = "error message";
        var resultWithError = Result<string>.Failure(statusCode, errorMsg);

        // Act
        var result = Result<int>.PropagateError(resultWithError);
        
        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }
    
    [Fact]
    public void PropagateError_WhenNonGenericResultWithErrorPassed_ShouldReturnResultWithError()
    {
        // Arrange
        const int statusCode = 500;
        const string errorMsg = "error message";
        var resultWithError = Result.Failure(statusCode, errorMsg);

        // Act
        var result = Result<int>.PropagateError(resultWithError);
        
        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Fact]
    public void PropagateError_WhenResultWithSuccessPassed_ShouldThrowArgumentException()
    {
        // Arrange
        var resultWithSuccess = Result<string>.Success("abc");

        // Act
        var act = () => Result<int>.PropagateError(resultWithSuccess);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
    
    [Fact]
    public void PropagateError_WhenNonGenericResultWithSuccessPassed_ShouldThrowArgumentException()
    {
        // Arrange
        var resultWithSuccess = Result.Success();

        // Act
        var act = () => Result<int>.PropagateError(resultWithSuccess);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
    
    [Fact]
    public void ToObjectResult_WhenGenericResultIsError_ShouldReturnObjectResultWithErrorDetails()
    {
        // Arrange
        const int statusCode = 404;
        const string errorMsg = "Not found";
        var result = Result<int>.Failure(statusCode, errorMsg);
        
        // Act
        var objectResult = result.ToObjectResult();
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(statusCode, objectResult.StatusCode);
        Assert.Equal(errorMsg, objectResult.Value);
    }
    
    [Fact]
    public void ToObjectResult_WhenGenericResultIsSuccess_ShouldReturnObjectResultWithDefaultSuccessCode()
    {
        // Arrange
        const int value = 42;
        var result = Result<int>.Success(value);
        
        // Act
        var objectResult = result.ToObjectResult();
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Equal(value, objectResult.Value);
    }
    
    [Fact]
    public void ToObjectResult_WhenGenericResultIsSuccessWithCustomStatusCode_ShouldReturnObjectResultWithSpecifiedCode()
    {
        // Arrange
        const string value = "Success data";
        var result = Result<string>.Success(value);
        const int customSuccessCode = StatusCodes.Status201Created;
        
        // Act
        var objectResult = result.ToObjectResult(customSuccessCode);
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(customSuccessCode, objectResult.StatusCode);
        Assert.Equal(value, objectResult.Value);
    }
    
    [Fact]
    public void ToObjectResult_WhenGenericResultWithNullValueIsSuccess_ShouldReturnObjectResultWithNullValue()
    {
        // Arrange
        var result = Result<string>.Success(null);
        
        // Act
        var objectResult = result.ToObjectResult();
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Null(objectResult.Value);
    }
}