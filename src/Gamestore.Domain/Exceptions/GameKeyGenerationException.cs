namespace Gamestore.Domain.Exceptions;

public class GameKeyGenerationException : ApiException
{
    public GameKeyGenerationException(string gameName)
        : base($"Failed to generate a unique game key for game '{gameName}'.")
    {
    }
}