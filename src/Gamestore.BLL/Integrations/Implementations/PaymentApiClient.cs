using System.Net.Http.Json;
using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.DTOs.Payments.Visa;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Integrations.Implementations;

public class PaymentApiClient : IPaymentApiClient
{
    private const string BaseUrl = "payments";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentApiClient> _logger;

    public PaymentApiClient(
        HttpClient httpClient,
        ILogger<PaymentApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BoxTransactionResponse> ProcessIBoxPaymentAsync(
        BoxTransactionRequest iboxRequest,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(iboxRequest);

        _logger.LogInformation(
            "Processing IBox payment for account '{AccountNumber}' with invoice number '{InvoiceNumber}'",
            iboxRequest.AccountNumber,
            iboxRequest.InvoiceNumber);

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ibox", iboxRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

            _logger.LogError("Failed to process IBox payment with invoice number '{InvoiceNumber}': {ErrorContent}", iboxRequest.InvoiceNumber, problemDetails);
            throw new PaymentProcessingException(
                (int)response.StatusCode,
                $"Failed to process IBox payment. {problemDetails.Title}",
                problemDetails.Title,
                problemDetails.Detail);
        }

        var result = await response.Content.ReadFromJsonAsync<BoxTransactionResponse>(cancellationToken);

        _logger.LogInformation(
            "Successfully processed IBox payment for account '{AccountNumber}' with invoice number '{InvoiceNumber}'",
            iboxRequest.AccountNumber,
            iboxRequest.InvoiceNumber);

        return result;
    }

    public async Task ProcessVisaPaymentAsync(VisaTransactionRequest visaRequest, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(visaRequest);

        _logger.LogInformation(
            "Processing Visa payment for card holder '{CardHolderName}' with amount {TransactionAmount}",
            visaRequest.CardHolderName,
            visaRequest.TransactionAmount);

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/visa", visaRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

            _logger.LogError("Failed to process Visa payment for card holder '{CardHolderName}': {ErrorContent}", visaRequest.CardHolderName, problemDetails);
            throw new PaymentProcessingException(
                (int)response.StatusCode,
                $"Failed to process Visa payment. {problemDetails.Title}",
                problemDetails.Title,
                problemDetails.Detail);
        }

        _logger.LogInformation(
            "Successfully processed Visa payment for card holder '{CardHolderName}' with amount {TransactionAmount}",
            visaRequest.CardHolderName,
            visaRequest.TransactionAmount);
    }
}