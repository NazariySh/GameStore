using Microsoft.AspNetCore.Http;

namespace Gamestore.Domain.Exceptions;

public class ConflictException : ApiException
{
    private const int DefaultStatusCode = StatusCodes.Status409Conflict;

    public ConflictException(string message)
        : base(DefaultStatusCode, message)
    {
    }
}