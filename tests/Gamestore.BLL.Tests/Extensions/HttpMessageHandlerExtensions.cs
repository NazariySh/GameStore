using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;

namespace Gamestore.BLL.Tests.Extensions;

public static class HttpMessageHandlerExtensions
{
    public static void SetupSendPost<TResponse>(
        this Mock<HttpMessageHandler> mockHandler,
        string requestUri,
        TResponse? response)
    {
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri(requestUri)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json"),
            });
    }

    public static void SetupSendPostFailed(
        this Mock<HttpMessageHandler> mockHandler,
        string requestUri,
        HttpStatusCode resultStatusCode,
        ProblemDetails problemDetails)
    {
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri(requestUri)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = resultStatusCode,
                Content = new StringContent(JsonSerializer.Serialize(problemDetails), Encoding.UTF8, "application/json"),
            });
    }

    public static void VerifyCalledOnce(this Mock<HttpMessageHandler> mockHandler, string requestUri)
    {
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }
}
