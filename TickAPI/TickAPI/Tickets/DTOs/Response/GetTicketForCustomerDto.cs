namespace TickAPI.Tickets.DTOs.Response;

public record GetTicketForCustomerDto
(
    string EventName,
    DateTime EventStartDate,
    DateTime EventEndDate
);