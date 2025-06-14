using TickAPI.Events.Models;

namespace TickAPI.Events.DTOs.Response;

public record GetEventDetailsResponseDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    List<GetEventResponseCategoryDto> Categories,
    List<GetEventDetailsResponseTicketTypeDto> TicketTypes,
    EventStatus Status,
    GetEventResponseAddressDto Address,
    string? ImageUrl
);