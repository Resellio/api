using TickAPI.Common.Result;

namespace TickAPI.Common.Pagination.Abstractions;

public interface IPaginationService
{
    public Result<PaginatedData<T>> Paginate<T>(ICollection<T> collection, int pageSize, int page);
}