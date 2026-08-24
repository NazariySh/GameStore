using System.Diagnostics;
using System.Text;

namespace Gamestore.WebApi.Middlewares;

public class RequestLoggingMiddleware : IMiddleware
{
    public const string CorrelationId = "CorrelationId";
    private const int MaxResponseBodyLength = 2048;

    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var timer = Stopwatch.StartNew();

        var correlationId = GetCorrelationId();
        context.Items[CorrelationId] = correlationId;

        var request = context.Request;
        var requestMethod = request.Method;
        var requestUrl = GetRequestUrl(request);
        var ipAddress = GetIpAddress(context);
        var requestBody = await GetRequestAsTextAsync(request);

        var responseBody = await CaptureResponseBodyAsync(context, next);

        timer.Stop();

        var statusCode = context.Response.StatusCode;

        _logger.LogInformation(
            """
            Request Log:
            Correlation Id: {CorrelationId}
            IP Address: {IpAddress}
            Request Method: {RequestMethod}
            Request Url: {RequestUrl}
            Status Code: {StatusCode}
            Request Body: {RequestBody}
            Response Body: {ResponseBody}
            Elapsed Time: {ElapsedMilliseconds} ms
            """,
            correlationId,
            ipAddress,
            requestMethod,
            requestUrl,
            statusCode,
            requestBody,
            responseBody,
            timer.ElapsedMilliseconds);
    }

    private static async Task<string> CaptureResponseBodyAsync(HttpContext context, RequestDelegate next)
    {
        var response = context.Response;
        var originalBodyStream = response.Body;

        await using var responseBodyStream = new MemoryStream();
        response.Body = responseBodyStream;

        try
        {
            await next(context);

            var responseBody = await GetResponseAsTextAsync(response);

            await responseBodyStream.CopyToAsync(originalBodyStream);

            return responseBody;
        }
        finally
        {
            response.Body = originalBodyStream;
        }
    }

    private static async Task<string> GetRequestAsTextAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return requestBody;
    }

    private static async Task<string> GetResponseAsTextAsync(HttpResponse response)
    {
        ResetStreamToBeginning(response.Body);

        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);

        var buffer = new char[MaxResponseBodyLength];
        var read = await reader.ReadBlockAsync(buffer, 0, MaxResponseBodyLength);
        var responseBody = new string(buffer, 0, read);

        if (!reader.EndOfStream)
        {
            responseBody += "...(truncated)";
        }

        ResetStreamToBeginning(response.Body);

        return responseBody;
    }

    private static void ResetStreamToBeginning(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
    }

    private static string GetIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
    }

    private static string GetRequestUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
    }

    private static string GetCorrelationId()
    {
        return Guid.NewGuid().ToString();
    }
}