using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Tests.Common.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnResultWithSuccess()
    {
        // Act
        var result = Result.Success();
        
        // Assert
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
        var result = Result.Failure(500, errorMsg);
        
        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }
    
    [Fact]
    public void PropagateError_WhenGenericResultWithErrorPassed_ShouldReturnResultWithError()
    {
        // Arrange
        const int statusCode = 500;
        const string errorMsg = "error message";
        var resultWithError = Result<int>.Failure(statusCode, errorMsg);

        // Act
        var result = Result.PropagateError(resultWithError);
        
        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }
    
    [Fact]
    public void PropagateError_WhenGenericResultWithSuccessPassed_ShouldThrowArgumentException()
    {
        // Arrange
        var resultWithSuccess = Result<int>.Success(123);

        // Act
        var act = () => Result<int>.PropagateError(resultWithSuccess);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
    
    [Fact]
    public void ToObjectResult_WhenResultIsError_ShouldReturnObjectResultWithErrorDetails()
    {
        // Arrange
        const int statusCode = 400;
        const string errorMsg = "Bad request";
        var result = Result.Failure(statusCode, errorMsg);
        
        // Act
        var objectResult = result.ToObjectResult();
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(statusCode, objectResult.StatusCode);
        Assert.Equal(errorMsg, objectResult.Value);
    }
    
    [Fact]
    public void ToObjectResult_WhenResultIsSuccess_ShouldReturnObjectResultWithDefaultSuccessCode()
    {
        // Arrange
        var result = Result.Success();
        
        // Act
        var objectResult = result.ToObjectResult();
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Null(objectResult.Value);
    }
    
    [Fact]
    public void ToObjectResult_WhenResultIsSuccessWithCustomStatusCode_ShouldReturnObjectResultWithSpecifiedCode()
    {
        // Arrange
        var result = Result.Success();
        const int customSuccessCode = StatusCodes.Status201Created;
        
        // Act
        var objectResult = result.ToObjectResult(customSuccessCode);
        
        // Assert
        Assert.IsType<ObjectResult>(objectResult);
        Assert.Equal(customSuccessCode, objectResult.StatusCode);
        Assert.Null(objectResult.Value);
    }
}