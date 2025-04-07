using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Pagination.Abstractions;

public interface IPaginationService
{
    public Task<Result<PaginationDetails>> GetPaginationDetailsAsync<T>(IQueryable<T> collection, int pageSize);
    public Task<Result<PaginatedData<T>>> PaginateAsync<T>(IQueryable<T> collection, int pageSize, int page);
    public PaginatedData<TTarget> MapData<TSource, TTarget>(PaginatedData<TSource> source, Func<TSource, TTarget> mapFunction);
}