using System.Net.Http.Json;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Integrations.Implementations;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(
        HttpClient httpClient,
        ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(AuthRequestDto authRequest, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(authRequest);

        _logger.LogInformation("Attempting to log in user with email '{Email}' using Auth API", authRequest.Email);

        var response = await _httpClient.PostAsJsonAsync("auth", authRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

            _logger.LogError("Failed to log in user with email '{Email}': {ErrorContent}", authRequest.Email, problemDetails);
            throw new ApiException(
                (int)response.StatusCode,
                $"Failed to log in user. {problemDetails.Title}",
                problemDetails.Title,
                problemDetails.Detail);
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken);

        _logger.LogInformation("Successfully logged in user with email '{Email}' using Auth API", authRequest.Email);

        return result;
    }
}