using TickAPI.Events.Models;
using TickAPI.Addresses.DTOs.Request;

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