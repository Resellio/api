using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;
using TickAPI.Common.Mail.Abstractions;
using TickAPI.Common.Results;

namespace TickAPI.Common.Mail.Services;

public class MailService : IMailService
{
    private SendGridClient _client;
    private EmailAddress _fromEmailAddress;
    
    public MailService(IConfiguration configuration)
    {
        var apiKey = configuration["SendGrid:ApiKey"];
        _client = new SendGridClient(apiKey);
        var fromEmail = configuration["SendGrid:FromEmail"];
        var fromName = configuration["SendGrid:FromName"];
        _fromEmailAddress = new EmailAddress(fromEmail, fromName);
    }

    public async Task<Result> SendTicketAsync(string toEmail, string toLogin, string eventName, byte[] pdfData)
    {
        var subject = $"Ticket for {eventName}";
        var toEmailAddress = new EmailAddress(toEmail, toLogin);
        var htmlContent = "<strong>Download your ticket from attachments</strong>";
        var msg = MailHelper.CreateSingleEmail(_fromEmailAddress, toEmailAddress, subject, null, htmlContent);
        var base64Content = Convert.ToBase64String(pdfData);
        msg.AddAttachment("ticket.pdf", base64Content, "application/pdf");
        var response = await _client.SendEmailAsync(msg).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return Result.Success();
        }
        return Result.Failure(500, "Error sending ticket");
    }
}