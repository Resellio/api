using TickAPI.Common.Mail.Models;
using TickAPI.Common.Results;

namespace TickAPI.Common.Mail.Abstractions;

public interface IMailService
{
    public Task<Result> SendTicketAsync(MailRecipient recipient, string eventName, byte[] pdfData);

    public Task<Result> SendMailAsync(IEnumerable<MailRecipient> recipients, string subject, string content, List<MailAttachment>? attachments);
}