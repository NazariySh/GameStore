using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Validators.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Games;

public class GameKeyGenerator : IGameKeyGenerator
{
    public const int MaxKeyLength = GameValidationRules.MaxKeyLength;
    private const char SuffixDelimiter = '_';
    private const int MaxRetries = 10;

    private readonly IRepository<Game> _gameRepository;
    private readonly INameNormalizer _nameNormalizer;
    private readonly ILogger<GameKeyGenerator> _logger;

    public GameKeyGenerator(
        IRepository<Game> gameRepository,
        INameNormalizer nameNormalizer,
        ILogger<GameKeyGenerator> logger)
    {
        _gameRepository = gameRepository;
        _nameNormalizer = nameNormalizer;
        _logger = logger;
    }

    public async Task<string> GenerateUniqueAsync(string gameName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to generate unique key for game '{GameName}'", gameName);

        Guard.AgainstNullOrWhiteSpace(gameName);

        var baseKey = GenerateGameKey(gameName);
        var key = baseKey;
        var suffix = 1;

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            if (await IsValidUniqueKeyAsync(key, cancellationToken))
            {
                _logger.LogInformation("Generated unique key '{Key}' for game '{GameName}'", key, gameName);

                return key;
            }

            _logger.LogWarning("Failed to generate unique key '{Key}' for game '{GameName}' on attempt {Attempt}. Retrying...", key, gameName, attempt + 1);
            key = CombineKeyWithSuffix(baseKey, suffix++);
        }

        _logger.LogError("Failed to generate a unique key for game '{GameName}' after {MaxRetries} attempts", gameName, MaxRetries);
        throw new GameKeyGenerationException(gameName);
    }

    private string GenerateGameKey(string gameName)
    {
        var normalizedKey = _nameNormalizer.Normalize(gameName);
        return Truncate(normalizedKey, MaxKeyLength);
    }

    private async Task<bool> IsValidUniqueKeyAsync(string key, CancellationToken cancellationToken)
    {
        return !string.IsNullOrWhiteSpace(key) && await IsUniqueKeyAsync(key, cancellationToken);
    }

    private Task<bool> IsUniqueKeyAsync(string key, CancellationToken cancellationToken)
    {
        return _gameRepository.NotExistsAsync(x => x.Key == key, cancellationToken);
    }

    private static string CombineKeyWithSuffix(string key, int suffix)
    {
        var suffixPart = $"{SuffixDelimiter}{suffix}";
        var allowedBaseLength = MaxKeyLength - suffixPart.Length;
        var normalizedKey = Truncate(key, allowedBaseLength);

        return $"{normalizedKey}{suffixPart}";
    }

    private static string Truncate(string input, int maxLength)
    {
        return input.Length > maxLength
            ? input[..maxLength]
            : input;
    }
}