using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Results;

public record Result
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public int StatusCode { get; }
    public string ErrorMsg { get; }
    
    private Result(bool isSuccess, int statusCode = StatusCodes.Status200OK, string errorMsg = "")
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        ErrorMsg = errorMsg;
    }

    public static Result Success()
    {
        return new Result(true);
    }

    public static Result Failure(int statusCode, string errorMsg)
    {
        return new Result(false, statusCode, errorMsg);
    }

    public static Result PropagateError(Result other)
    {
        if (other.IsSuccess)
        {
            throw new ArgumentException("Trying to propagate error from successful value");
        }
        
        return Failure(other.StatusCode, other.ErrorMsg);
    }

    public static Result PropagateError<TE>(Result<TE> other)
    {
        if (other.IsSuccess)
        {
            throw new ArgumentException("Trying to propagate error from successful value");
        }

        return Failure(other.StatusCode, other.ErrorMsg);
    }
}