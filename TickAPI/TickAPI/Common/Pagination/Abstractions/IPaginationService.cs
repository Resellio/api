using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Pagination.Abstractions;

public interface IPaginationService
{
    public Result<PaginationDetails> GetPaginationDetails<T>(ICollection<T> collection, int pageSize);
    public Result<PaginatedData<T>> Paginate<T>(ICollection<T> collection, int pageSize, int page);
}