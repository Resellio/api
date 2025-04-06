using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Pagination.Services;

public class PaginationService : IPaginationService
{
    public async Task<Result<PaginationDetails>> GetPaginationDetailsAsync<T>(IQueryable<T> collection, int pageSize)
    {
        if (pageSize <= 0)
        {
            return Result<PaginationDetails>.Failure(StatusCodes.Status400BadRequest, $"'pageSize' param must be > 0, got: {pageSize}");
        }

        var allElementsCount = collection.Provider is IAsyncQueryProvider ? await collection.CountAsync() : collection.Count();
        var maxPageNumber = Math.Max((int)Math.Ceiling(1.0 * allElementsCount / pageSize) - 1, 0);

        var paginationDetails = new PaginationDetails(maxPageNumber, allElementsCount);

        return Result<PaginationDetails>.Success(paginationDetails);
    }

    public async Task<Result<PaginatedData<T>>> PaginateAsync<T>(IQueryable<T> collection, int pageSize, int page)
    {
        if (pageSize <= 0)
        {
            return Result<PaginatedData<T>>.Failure(StatusCodes.Status400BadRequest, $"'pageSize' param must be > 0, got: {pageSize}");
        }

        if (page < 0)
        {
            return Result<PaginatedData<T>>.Failure(StatusCodes.Status400BadRequest, $"'page' param must be >= 0, got: {page}");
        }

        var paginationDetailsResult = await GetPaginationDetailsAsync(collection, pageSize);
        if (paginationDetailsResult.IsError)
        {
            return Result<PaginatedData<T>>.PropagateError(paginationDetailsResult);
        }

        var paginationDetails = paginationDetailsResult.Value!;
        
        if (page > paginationDetails.MaxPageNumber)
        {
            return Result<PaginatedData<T>>.Failure(StatusCodes.Status400BadRequest,
                $"'page' param must be <= {paginationDetails.MaxPageNumber}, got: {page}");
        }

        var paginatedQuery = collection.Skip(page * pageSize).Take(pageSize);
        var data = collection.Provider is IAsyncQueryProvider ? await paginatedQuery.ToListAsync() : paginatedQuery.ToList();
        var hasPreviousPage = page > 0;
        var hasNextPage = page < paginationDetails.MaxPageNumber;

        var paginatedData = new PaginatedData<T>(data, page, pageSize, hasNextPage, hasPreviousPage, paginationDetails);

        return Result<PaginatedData<T>>.Success(paginatedData);
    }

    public PaginatedData<TTarget> MapData<TSource, TTarget>(PaginatedData<TSource> source, Func<TSource, TTarget> mapFunction)
    {
        var newData = source.Data.Select(mapFunction).ToList();
        return new PaginatedData<TTarget>(newData, source.PageNumber, source.PageSize, source.HasNextPage, source.HasPreviousPage, source.PaginationDetails);
    }
}