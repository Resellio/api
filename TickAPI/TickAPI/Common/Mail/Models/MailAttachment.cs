namespace TickAPI.Common.Mail.Models;

public class MailAttachment
{
    public string fileName { get; set; }
    public string base64Content { get; set; }
    public string fileType  { get; set; }
}