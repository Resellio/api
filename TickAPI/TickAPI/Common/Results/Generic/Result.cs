﻿using Microsoft.AspNetCore.Mvc;

namespace TickAPI.Common.Results.Generic;

public record Result<T>
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public T? Value { get; }
    public int StatusCode { get; }
    public string ErrorMsg { get; }
    
    private Result(bool isSuccess, T? value = default, int statusCode = StatusCodes.Status200OK, string errorMsg = "")
    {
        IsSuccess = isSuccess;
        Value = value;
        StatusCode = statusCode;
        ErrorMsg = errorMsg;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value);
    }

    public static Result<T> Failure(int statusCode, string errorMsg)
    {
        return new Result<T>(false, default, statusCode, errorMsg);
    }
    
    public static Result<T> PropagateError(Result other)
    {
        if (other.IsSuccess)
        {
            throw new ArgumentException("Trying to propagate error from successful value");
        }

        return Failure(other.StatusCode, other.ErrorMsg);
    }

    public static Result<T> PropagateError<TE>(Result<TE> other)
    {
        if (other.IsSuccess)
        {
            throw new ArgumentException("Trying to propagate error from successful value");
        }

        return Failure(other.StatusCode, other.ErrorMsg);
    }
    
    public ObjectResult ToObjectResult(int successCode = StatusCodes.Status200OK)
    {
        if (IsError)
        {
            return new ObjectResult(ErrorMsg)
            {
                StatusCode = StatusCode
            };
        }

        return new ObjectResult(Value)
        {
            StatusCode = successCode
        };
    }
}