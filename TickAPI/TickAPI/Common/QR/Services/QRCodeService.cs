using QRCoder;
using TickAPI.Common.QR.Abstractions;

namespace TickAPI.Common.QR.Services;

public class QRCodeService : IQRCodeService
{
    public byte[] GenerateQrCode(Guid ticketId)
    {
        var qrGenerator = new QRCodeGenerator();
        var url = "localhost:5124/scan/" + ticketId;
        var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var  qrCodeImage = qrCode.GetGraphic(20);
        return qrCodeImage;
    }
}