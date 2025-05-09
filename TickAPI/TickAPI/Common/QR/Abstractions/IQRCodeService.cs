namespace TickAPI.Common.QR.Abstractions;

public interface IQRCodeService
{
    public byte[] GenerateQrCode(Guid ticketId);
}