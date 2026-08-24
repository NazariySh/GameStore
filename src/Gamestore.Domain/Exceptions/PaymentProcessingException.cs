namespace Gamestore.Domain.Exceptions;

public class PaymentProcessingException : ApiException
{
    public PaymentProcessingException(string message)
        : base(message)
    {
    }

    public PaymentProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PaymentProcessingException(int statusCode, string message, string? title, string? detail)
        : base(statusCode, message, title, detail)
    {
    }

    public PaymentProcessingException(int statusCode, string message, string? title, string? detail, Exception innerException)
        : base(statusCode, message, title, detail, innerException)
    {
    }
}