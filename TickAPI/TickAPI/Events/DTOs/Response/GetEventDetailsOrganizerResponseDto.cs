using TickAPI.Events.Models;
using TickAPI.Events.DTOs.Response;

namespace TickAPI.Events.DTOs.Response;


public record GetEventDetailsOrganizerResponseDto(
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
    decimal Revenue,
    int SoldTicketsCount
    );
    
  