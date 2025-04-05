using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Response;

public record GetEventResponseDto(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    List<GetEventResponseCategoryDto> Categories,
    EventStatus Status,
    GetEventResponseAddressDto Addres
);
