using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games;

public class GameCreateDtoValidator : AbstractValidator<GameCreateDto>
{
    private readonly IRepository<Game> _gameRepository;

    public GameCreateDtoValidator(
        IRepository<Game> gameRepository,
        IValidator<GameCreateUpdateDto> baseValidator,
        GameKeyValidator gameKeyValidator)
    {
        _gameRepository = gameRepository;

        Include(baseValidator);

        RuleFor(x => x.Key)
            .SetValidator(gameKeyValidator!)
            .MustAsync(BeUniqueKeyAsync)
            .WithMessage("Game with this key already exists.")
            .When(NotEmptyKey);
    }

    private static bool NotEmptyKey(GameCreateDto game)
    {
        return !string.IsNullOrEmpty(game.Key);
    }

    private Task<bool> BeUniqueKeyAsync(string? key, CancellationToken cancellationToken)
    {
        return _gameRepository.NotExistsAsync(
            g => g.Key == key,
            cancellationToken);
    }
}