using Microsoft.Extensions.Diagnostics.HealthChecks;
using TickAPI.Common.Payment.Abstractions;
using TickAPI.Common.Payment.Extensions;

namespace TickAPI.Common.Payment.Health;

public class PaymentGatewayHealthCheck : IHealthCheck
{
    private readonly IPaymentGatewayService _paymentGateway;

    public PaymentGatewayHealthCheck(IPaymentGatewayService paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var status = await _paymentGateway.HealthCheck();
        if (status.IsHealthy())
        {
            return HealthCheckResult.Healthy("Payment gateway is reachable.");
        }
        return HealthCheckResult.Unhealthy("Payment gateway is not reachable.");
    }
}
