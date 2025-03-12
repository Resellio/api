using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Result;

namespace TickAPI.Common.Pagination.Services;

public class PaginationService : IPaginationService
{
    public Result<PaginationDetails> GetPaginationDetails<T>(ICollection<T> collection, int pageSize)
    {
        if (pageSize <= 0)
        {
            return Result<PaginationDetails>.Failure(StatusCodes.Status400BadRequest, $"'pageSize' param must be > 0, got: {pageSize}");
        }
        
        var allElementsCount = collection.Count;
        var maxPageNumber = Math.Max((int)Math.Ceiling(1.0 * allElementsCount / pageSize) - 1, 0);

        var paginationDetails = new PaginationDetails(maxPageNumber, allElementsCount);

        return Result<PaginationDetails>.Success(paginationDetails);
    }

    public Result<PaginatedData<T>> Paginate<T>(ICollection<T> collection, int pageSize, int page)
    {
        if (pageSize <= 0)
        {
            return Result<PaginatedData<T>>.Failure(StatusCodes.Status400BadRequest, $"'pageSize' param must be > 0, got: {pageSize}");
        }

        if (page < 0)
        {
            return Result<PaginatedData<T>>.Failure(StatusCodes.Status400BadRequest, $"'page' param must be >= 0, got: {page}");
        }

        var paginationDetailsResult = GetPaginationDetails(collection, pageSize);
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
        
        var data = collection.Skip(page * pageSize).Take(pageSize).ToList();
        var hasPreviousPage = page > 0;
        var hasNextPage = page < paginationDetails.MaxPageNumber;

        var paginatedData = new PaginatedData<T>(data, page, pageSize, hasNextPage, hasPreviousPage, paginationDetails);

        return Result<PaginatedData<T>>.Success(paginatedData);
    }
}