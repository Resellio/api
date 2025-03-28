using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Request;

public record CreateEventDto
(
    string Name,
    string Description,
    string StartDate,
    string EndDate,
    uint? MinimumAge,
    EventStatus EventStatus,
    CreateAddressDto CreateAddress
);