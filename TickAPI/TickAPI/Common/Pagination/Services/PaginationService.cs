using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Result;

namespace TickAPI.Common.Pagination.Services;

public class PaginationService : IPaginationService
{
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

        var totalCount = collection.Count;
        var data = collection.Skip(page * pageSize).Take(pageSize).ToList();
        var hasPreviousPage = page > 0 && ((page - 1) * pageSize) < totalCount;
        var hasNextPage = ((page + 1) * pageSize) < totalCount;

        var paginatedData = new PaginatedData<T>(data, page, pageSize, hasNextPage, hasPreviousPage);

        return Result<PaginatedData<T>>.Success(paginatedData);
    }
}