using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Tests.Common.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnResultWithSuccess()
    {
        var result = Result.Success();
        
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal("", result.ErrorMsg);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void Failure_ShouldReturnResultWithError()
    {
        const int statusCode = 500;
        const string errorMsg = "example error msg";
        
        var result = Result.Failure(500, errorMsg);
        
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Fact]
    public void PropagateError_WhenResultWithErrorPassed_ShouldReturnResultWithError()
    {
        const int statusCode = 500;
        const string errorMsg = "error message";
        var resultWithError = Result.Failure(statusCode, errorMsg);

        var result = Result.PropagateError(resultWithError);
        
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }
    
    [Fact]
    public void PropagateError_WhenGenericResultWithErrorPassed_ShouldReturnResultWithError()
    {
        const int statusCode = 500;
        const string errorMsg = "error message";
        var resultWithError = Result<int>.Failure(statusCode, errorMsg);

        var result = Result.PropagateError(resultWithError);
        
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.ErrorMsg);
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Fact]
    public void PropagateError_WhenResultWithSuccessPassed_ShouldThrowArgumentException()
    {
        var resultWithSuccess = Result.Success();

        var act = () => Result.PropagateError(resultWithSuccess);

        Assert.Throws<ArgumentException>(act);
    }
    
    [Fact]
    public void PropagateError_WhenGenericResultWithSuccessPassed_ShouldThrowArgumentException()
    {
        var resultWithSuccess = Result<int>.Success(123);

        var act = () => Result<int>.PropagateError(resultWithSuccess);

        Assert.Throws<ArgumentException>(act);
    }
}