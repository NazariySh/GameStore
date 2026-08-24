namespace Gamestore.Domain.Exceptions;

public class GameOutOfStockException : ConflictException
{
    public GameOutOfStockException(string gameKey)
        : base($"Game with key '{gameKey}' is out of stock.")
    {
    }
}