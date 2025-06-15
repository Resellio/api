using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Mail.Models;
using TickAPI.Common.Results;
using TickAPI.Customers.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.Common.Mail.Services;

public class MailService : IMailService
{
    private readonly SendGridClient _client;
    private readonly EmailAddress _fromEmailAddress;
    
    public MailService(IConfiguration configuration)
    {
        var apiKey = configuration["SendGrid:ApiKey"];
        _client = new SendGridClient(apiKey);
        var fromEmail = configuration["SendGrid:FromEmail"];
        var fromName = configuration["SendGrid:FromName"];
        _fromEmailAddress = new EmailAddress(fromEmail, fromName);
    }
    
    public async Task<Result> SendTicketsAsync(Customer customer, List<Ticket> tickets)
    {
        var subject = "Your New Tickets";
        var htmlContent = new StringBuilder();
        htmlContent.AppendLine("<strong>You have purchased tickets for following events:</strong><br/><ul>");
        
        foreach (var ticket in tickets)
        {
            var eventName = ticket.Type.Event.Name;
            var eventDate = ticket.Type.Event.StartDate.ToString("yyyy-MM-dd");
           
            htmlContent.AppendLine(
                $"<li><b>{eventName}</b> on {eventDate} (ticket: {ticket.Type.Description})</li>"
            );
        }

        htmlContent.AppendLine("</ul>");

        var recipient = new MailRecipient(customer.Email, customer.FirstName);
        return await SendMailAsync([recipient], subject, htmlContent.ToString(), []);
    }
    
    public async Task<Result> SendMailAsync(IEnumerable<MailRecipient> recipients, string subject, string content,
        List<MailAttachment>? attachments = null)
    {
        var toEmailAddresses = recipients.Select(r => new EmailAddress(r.Email, r.Login)).ToList();
        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(_fromEmailAddress, toEmailAddresses, subject, null, content);
        
        if (attachments != null)
        {
            foreach (var a in attachments)
            {
                msg.AddAttachment(a.FileName, a.Base64Content, a.FileType);
            }
        }
        
        var response = await _client.SendEmailAsync(msg).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return Result.Success();
        }
        return Result.Failure(500, "Error sending email");
    }

}