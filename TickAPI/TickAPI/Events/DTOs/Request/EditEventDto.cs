using TickAPI.Addresses.DTOs.Request;
using TickAPI.Categories.DTOs.Request;
using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Request;

public record EditEventDto(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    List<EditEventCategoryDto> Categories,
    EventStatus EventStatus,
    CreateAddressDto EditAddress
);
