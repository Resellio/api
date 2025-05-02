using TickAPI.Common.Payment.Models;

namespace TickAPI.Common.Payment.Extensions;

public static class PaymentGatewayHealthStatusExtensions
{
    public static bool IsHealthy(this PaymentGatewayHealthStatus response)
    {
        return response.Status.Equals("ok", StringComparison.CurrentCultureIgnoreCase);
    }
}
