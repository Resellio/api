namespace TickAPI.Common.Pagination.Responses;

public record PaginatedData<T>(
    List<T> Data,
    // First page should have number '0'
    int PageNumber,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage,
    PaginationDetails PaginationDetails 
);