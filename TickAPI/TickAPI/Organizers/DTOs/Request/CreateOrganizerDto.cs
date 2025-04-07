namespace TickAPI.Organizers.DTOs.Request;

public record CreateOrganizerDto(
    string FirstName,
    string LastName,
    string DisplayName
);