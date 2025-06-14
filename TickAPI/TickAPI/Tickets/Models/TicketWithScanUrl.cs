namespace TickAPI.Tickets.Models;

public class TicketWithScanUrl
{
    public TicketWithScanUrl(Ticket ticket, string scanUrl)
    {
        Ticket = ticket;
        ScanUrl = scanUrl;
    }

    public Ticket Ticket { get; set; }
    public string ScanUrl { get; set; }
}
