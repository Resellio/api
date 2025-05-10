using QRCoder;
using TickAPI.Common.QR.Abstractions;

namespace TickAPI.Common.QR.Services;

public class QRCodeService : IQRCodeService
{
    public byte[] GenerateQrCode(string url)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var qrCodeImage = qrCode.GetGraphic(20);
        return qrCodeImage;
    }
}