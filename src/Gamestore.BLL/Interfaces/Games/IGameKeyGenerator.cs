namespace Gamestore.BLL.Interfaces.Games;

public interface IGameKeyGenerator
{
    Task<string> GenerateUniqueAsync(string gameName, CancellationToken cancellationToken = default);
}