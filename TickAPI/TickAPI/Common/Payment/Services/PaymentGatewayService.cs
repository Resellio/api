﻿using System.Text;
using System.Text.Json;
using TickAPI.Common.Payment.Abstractions;
using TickAPI.Common.Payment.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Payment.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentGatewayService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PaymentGatewayHealthStatus> HealthCheck()
    {
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["PaymentGateway:Url"]!;
        var url = $"{baseUrl}/health";
        var response = await client.GetAsync(url);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var status = JsonSerializer.Deserialize<PaymentGatewayHealthStatus>(jsonResponse, new JsonSerializerOptions());
        return status!;
    }

    public async Task<Result<PaymentResponsePG>> ProcessPayment(PaymentRequestPG request)
    {
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["PaymentGateway:Url"]!;
        var url = $"{baseUrl}/payments";
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = JsonSerializer.Deserialize<PaymentErrorResponsePG>(jsonResponse, new JsonSerializerOptions());
            return Result<PaymentResponsePG>.Failure((int)response.StatusCode, errorResponse!.Error);
        }

        var successResponse = JsonSerializer.Deserialize<PaymentResponsePG>(jsonResponse, new JsonSerializerOptions());
        return Result<PaymentResponsePG>.Success(successResponse!);
    }
}
