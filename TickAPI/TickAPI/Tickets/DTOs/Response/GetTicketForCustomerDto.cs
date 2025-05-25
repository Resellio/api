namespace TickAPI.Tickets.DTOs.Response;

public record GetTicketForCustomerDto
(
    Guid TicketId,
    string EventName,
    DateTime EventStartDate,
    DateTime EventEndDate,
    bool Used
);