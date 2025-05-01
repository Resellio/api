using TickAPI.Common.Payment.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Payment.Abstractions;

public interface IPaymentGatewayService
{
    Task<Result<PaymentResponsePG>> ProcessPayment(PaymentRequestPG request);
}
