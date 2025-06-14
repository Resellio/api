using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Mail.Models;
using TickAPI.Common.QR.Abstractions;
using TickAPI.Common.Results;
using TickAPI.Customers.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.Common.Mail.Services;

public class MailService : IMailService
{
    private readonly SendGridClient _client;
    private readonly EmailAddress _fromEmailAddress;
    private readonly IQRCodeService _qrCodeService;
    
    public MailService(IConfiguration configuration, IQRCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
        var apiKey = configuration["SendGrid:ApiKey"];
        _client = new SendGridClient(apiKey);
        var fromEmail = configuration["SendGrid:FromEmail"];
        var fromName = configuration["SendGrid:FromName"];
        _fromEmailAddress = new EmailAddress(fromEmail, fromName);
    }
    
    public async Task<Result> SendTicketsAsync(Customer customer, List<TicketWithScanUrl> tickets)
    {
        var subject = "Your New Tickets";
        var htmlContent = new StringBuilder();
        htmlContent.AppendLine("<strong>Here are your tickets:</strong><br/><ul>");

        var attachments = new List<MailAttachment>();

        foreach (var tWithScanUrl in tickets)
        {
            var ticket = tWithScanUrl.Ticket;
            var eventName = ticket.Type.Event.Name;
            var eventDate = ticket.Type.Event.StartDate.ToString("yyyy-MM-dd");
           
            htmlContent.AppendLine(
                $"<li>Ticket for event <b>{eventName}</b> on {eventDate} "
            );
            
            var pdfData = _qrCodeService.GenerateQrCode(tWithScanUrl.ScanUrl);
            
            var base64Content = Convert.ToBase64String(pdfData);
            attachments.Add(new MailAttachment($"ticket_{ticket.Id}.pdf", base64Content, "application/pdf"));
        }

        htmlContent.AppendLine("</ul>");

        var recipient = new MailRecipient(customer.Email, customer.FirstName);
        return await SendMailAsync([recipient], subject, htmlContent.ToString(), attachments);
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