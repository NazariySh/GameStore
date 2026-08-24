using Gamestore.Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Gamestore.Domain.Exceptions;

public class NotFoundException : ApiException
{
    private const int DefaultStatusCode = StatusCodes.Status404NotFound;

    public NotFoundException(string message)
        : base(DefaultStatusCode, message)
    {
    }

    public NotFoundException(string name, Guid id)
        : base(DefaultStatusCode, $"{name} with Id '{id}' not found.")
    {
    }

    public NotFoundException(string name, EntityId id)
        : base(DefaultStatusCode, $"{name} with Id '{id}' not found.")
    {
    }
}