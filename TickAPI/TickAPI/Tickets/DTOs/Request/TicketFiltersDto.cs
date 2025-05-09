namespace TickAPI.Tickets.DTOs.Request;

public record TicketFiltersDto
{
    public bool usedOnly { get; set; }
    public bool unusedOnly { get; set; }
    public bool forResellOnly { get; set; }
    public bool notForResellOnly { get; set; }
    public string ? EventName { get; set; }
}