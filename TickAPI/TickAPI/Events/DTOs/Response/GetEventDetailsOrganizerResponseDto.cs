namespace TickAPI.Events.DTOs.Response;

public record GetEventDetailsOrganizerResponseDto(
    GetEventDetailsResponseDto EventDetails,
    decimal Revenue,
    int SoldTicketsCount
    );