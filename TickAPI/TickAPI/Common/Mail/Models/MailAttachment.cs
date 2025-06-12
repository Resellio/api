namespace TickAPI.Common.Mail.Models;

public record MailAttachment(
    string FileName,
    string Base64Content,
    string FileType
);