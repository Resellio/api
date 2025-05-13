namespace TickAPI.Organizers.DTOs.Response;

public record GetUnverifiedOrganizerResponseDto(
    string Email,
    string FirstName,
    string? LastName,
    string DisplayName
);
