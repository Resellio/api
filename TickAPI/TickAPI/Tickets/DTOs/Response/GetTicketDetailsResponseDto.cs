namespace TickAPI.Tickets.DTOs.Response;

public class GetTicketDetailsResponseDto
{
    public string NameOnTicket { get; set; }
    public string? Seats { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public string EventName {get; set;}
    public string OrganizerName {get; set;}
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}