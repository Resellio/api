namespace TickAPI.Common.Pagination.Responses;

public record PaginationDetails(
    int MaxPageNumber,
    int AllElementsCount
);