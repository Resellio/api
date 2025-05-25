namespace TickAPI.Events.DTOs.Request;

public record SendMessageToParticipantsDto(
    string Subject,
    string Message
);
