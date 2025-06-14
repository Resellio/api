using TickAPI.Events.Models;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Categories.DTOs.Request;
using TickAPI.TicketTypes.DTOs.Request;

namespace TickAPI.Events.DTOs.Request;

public record CreateEventDto(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    uint? MinimumAge,
    List<CreateEventCategoryDto> Categories,
    List<CreateEventTicketTypeDto> TicketTypes,
    EventStatus EventStatus,
    CreateAddressDto CreateAddress,
    IFormFile? Image
);