using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Response;

public record GetEventResponseDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    decimal MinimumPrice,
    decimal MaximumPrice,
    List<GetEventResponseCategoryDto> Categories,
    EventStatus Status,
    GetEventResponseAddressDto Address
);
