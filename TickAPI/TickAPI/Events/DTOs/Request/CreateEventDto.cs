using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Request;

public record CreateEventDto
(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    EventStatus EventStatus,
    CreateAddressDto CreateAddress
);