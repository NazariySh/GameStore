using Microsoft.AspNetCore.Http;

namespace Gamestore.Domain.Exceptions;

public class ApiException : Exception
{
    private const int DefaultStatusCode = StatusCodes.Status500InternalServerError;

    public ApiException(string message)
        : this(DefaultStatusCode, message)
    {
    }

    public ApiException(string message, Exception innerException)
        : this(DefaultStatusCode, message, innerException)
    {
    }

    public ApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(int statusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public ApiException(int statusCode, string message, string? title, string? detail)
        : this(statusCode, message)
    {
        Title = title;
        Detail = detail;
    }

    public ApiException(int statusCode, string message, string? title, string? detail, Exception innerException)
        : this(statusCode, message, innerException)
    {
        Title = title;
        Detail = detail;
    }

    public int StatusCode { get; }

    public string? Title { get; }

    public string? Detail { get; }
}