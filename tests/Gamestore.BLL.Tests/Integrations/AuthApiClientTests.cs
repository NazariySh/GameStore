using System.Net;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Integrations.Implementations;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Integrations;

public class AuthApiClientTests
{
    private const string BaseUrl = "https://api.auth.com/";

    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly AuthApiClient _authApiClient;

    public AuthApiClientTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_mockHandler.Object)
        {
            BaseAddress = new Uri(BaseUrl),
        };
        _authApiClient = new AuthApiClient(
            httpClient,
            Mock.Of<ILogger<AuthApiClient>>());
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        var act = () => _authApiClient.LoginAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowApiException_WhenResponseIsNotSuccessful()
    {
        var request = GetAuthRequest();
        var problemDetails = new ProblemDetails
        {
            Title = "Authentication error",
            Detail = "Invalid email or password.",
        };

        var statusCode = HttpStatusCode.Unauthorized;

        _mockHandler.SetupSendPostFailed($"{BaseUrl}auth", statusCode, problemDetails);

        var act = () => _authApiClient.LoginAsync(request);

        var exception = await Assert.ThrowsAsync<ApiException>(act);

        Assert.Equal((int)statusCode, exception.StatusCode);
        Assert.Equal($"Failed to log in user. {problemDetails.Title}", exception.Message);
        Assert.Equal(problemDetails.Title, exception.Title);
        Assert.Equal(problemDetails.Detail, exception.Detail);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccessResponse_WhenRequestIsValid()
    {
        var request = GetAuthRequest();
        var expectedResponse = new AuthResponseDto
        {
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe",
        };

        _mockHandler.SetupSendPost($"{BaseUrl}auth", expectedResponse);

        var result = await _authApiClient.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equivalent(expectedResponse, result);
    }

    private static AuthRequestDto GetAuthRequest()
    {
        return new AuthRequestDto
        {
            Email = "user@example.com",
            Password = "P@ssw0rd123!",
        };
    }
}