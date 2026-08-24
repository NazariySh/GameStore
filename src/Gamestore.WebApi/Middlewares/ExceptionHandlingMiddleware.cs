using System.Text;
using FluentValidation;
using Gamestore.Domain.Exceptions;
using Microsoft.AspNetCore.WebUtilities;

namespace Gamestore.WebApi.Middlewares;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private const string ResponseContentType = "text/plain";

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        LogException(exception, context);

        context.Response.ContentType = ResponseContentType;
        context.Response.StatusCode = GetStatusCode(exception);

        return context.Response.WriteAsync(GetErrorResponse(exception));
    }

    private static string GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => CreateValidationErrorResponse(validationException),
            ApiException apiException => CreateApiErrorResponse(apiException),
            _ => CreateDefaultErrorResponse(exception),
        };
    }

    private static string CreateValidationErrorResponse(ValidationException exception)
    {
        var errors = GetValidationErrors(exception);
        var message = string.Join("; ", errors.SelectMany(x => x.Value.Select(msg => $"'{x.Key}': '{msg}'")));
        return FormatErrorResponse("Validation Failed", message);
    }

    private static Dictionary<string, string[]> GetValidationErrors(ValidationException exception)
    {
        return exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(e => e.ErrorMessage).ToArray());
    }

    private static string CreateApiErrorResponse(ApiException exception)
    {
        var title = exception.Title ?? GetTitleForStatusCode(exception.StatusCode);
        var message = exception.Detail ?? exception.Message;
        return FormatErrorResponse(title, message);
    }

    private static string CreateDefaultErrorResponse(Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var title = GetTitleForStatusCode(statusCode);
        return FormatErrorResponse(title, exception.Message);
    }

    private static string FormatErrorResponse(string title, string message)
    {
        return $"{title}: {message}";
    }

    private void LogException(Exception exception, HttpContext context)
    {
        var correlationId = GetCorrelationId(context);
        var sb = new StringBuilder();
        sb.AppendLine("Exception occurred:");
        if (!string.IsNullOrEmpty(correlationId))
        {
            sb.AppendLine($"Correlation Id: {correlationId}");
        }

        sb.AppendLine($"Request Method: {context.Request.Method}");
        sb.AppendLine($"Request Path: {context.Request.Path}");
        AppendExceptionDetails(sb, exception);

        _logger.LogError(sb.ToString());
    }

    private static void AppendExceptionDetails(StringBuilder sb, Exception exception)
    {
        sb.AppendLine($"Exception Type: {exception.GetType().FullName}");
        sb.AppendLine($"Exception Message: {exception.Message}");
        sb.AppendLine($"Stack Trace: {exception.StackTrace}");

        if (exception.InnerException is not null)
        {
            sb.AppendLine("Inner Exception:");
            AppendExceptionDetails(sb, exception.InnerException);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue(RequestLoggingMiddleware.CorrelationId, out var correlationId)
            ? correlationId?.ToString()
            : string.Empty;
    }

    private static string GetTitleForStatusCode(int statusCode)
    {
        return ReasonPhrases.GetReasonPhrase(statusCode);
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            ApiException apiException => apiException.StatusCode,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}