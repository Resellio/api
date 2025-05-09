using Microsoft.AspNetCore.Components.RenderTree;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace TickAPI.Common.QR.Services;

public class QRCodeService
{
    public byte[] GenerateQrCode(Guid ticketId)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(ticketId.ToString(), QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var  qrCodeImage = qrCode.GetGraphic(20);
        return qrCodeImage;
    }
}