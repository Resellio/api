using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Response;

public record GetEventResponseDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    GetEventResponsePriceInfoDto MinimumPrice,
    GetEventResponsePriceInfoDto MaximumPrice,
    List<GetEventResponseCategoryDto> Categories,
    EventStatus Status,
    GetEventResponseAddressDto Address
);
