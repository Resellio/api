using TickAPI.Common.Mail.Models;
using TickAPI.Common.Results;
using TickAPI.Customers.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.Common.Mail.Abstractions;

public interface IMailService
{
    public Task<Result> SendTicketsAsync(Customer customer, List<Ticket> tickets);
    public Task<Result> SendMailAsync(IEnumerable<MailRecipient> recipients, string subject, string content, List<MailAttachment>? attachments);
}