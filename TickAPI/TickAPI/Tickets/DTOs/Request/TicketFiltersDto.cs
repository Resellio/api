namespace TickAPI.Tickets.DTOs.Request;

public enum UsageFilter
{
    OnlyUsed,
    OnlyNotUsed
}

public enum ResellFilter
{
    OnlyForResell,
    OnlyNotForResell
}

public record TicketFiltersDto
(
    UsageFilter? Usage,
    ResellFilter? Resell,
    string? EventName
);