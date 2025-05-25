namespace TickAPI.Organizers.DTOs.Response;

public record AboutMeOrganizerResponseDto(
    string Email,
    string FirstName,
    string? LastName,
    string DisplayName,
    bool IsVerified,
    DateTime CreationDate
);