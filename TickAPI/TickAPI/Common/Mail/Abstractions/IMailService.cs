using TickAPI.Common.Mail.Models;
using TickAPI.Common.Results;

namespace TickAPI.Common.Mail.Abstractions;

public interface IMailService
{
    public Task<Result> SendTicketAsync(string toEmail, string toLogin, string eventName, byte[] pdfData);

    public Task<Result> SendMailAsync(string toEmail, string toLogin, string subject, string content,
        List<MailAttachment>? attachments);
}