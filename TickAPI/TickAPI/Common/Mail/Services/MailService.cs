using SendGrid;
using SendGrid.Helpers.Mail;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Mail.Models;
using TickAPI.Common.Results;

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

    public async Task<Result> SendTicketAsync(MailRecipient recipient, string eventName, byte[] pdfData)
    {
        var subject = $"Ticket for {eventName}";
        var htmlContent = "<strong>Download your ticket from attachments</strong>";
        var base64Content = Convert.ToBase64String(pdfData);
        List<MailAttachment> attachments = [
            new MailAttachment("ticket.pdf", base64Content, "application/pdf")
        ];
        var res = await SendMailAsync([recipient], subject, htmlContent, attachments);
        return res;
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