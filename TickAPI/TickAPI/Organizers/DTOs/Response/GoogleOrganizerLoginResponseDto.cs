namespace TickAPI.Organizers.DTOs.Response;

public record GoogleOrganizerLoginResponseDto(
    string Token,
    bool IsNewOrganizer,
    bool IsVerified
);