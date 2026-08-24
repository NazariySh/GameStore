using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games;

public class GameUpdateDtoValidator : AbstractValidator<GameUpdateDto>
{
    private readonly IRepository<Game> _gameRepository;

    public GameUpdateDtoValidator(
        IRepository<Game> gameRepository,
        IValidator<GameCreateUpdateDto> baseValidator,
        GameKeyValidator gameKeyValidator)
    {
        _gameRepository = gameRepository;

        Include(baseValidator);

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .SetValidator(gameKeyValidator);

        RuleFor(x => x)
            .MustAsync(BeUniqueKeyAsync)
            .WithMessage("Game with this key already exists.")
            .OverridePropertyName(x => x.Key);
    }

    private async Task<bool> BeUniqueKeyAsync(GameUpdateDto game, CancellationToken cancellationToken)
    {
        bool isUniqueKey;

        if (game.Id.IsPrimary)
        {
            isUniqueKey = await _gameRepository.NotExistsAsync(
                g => g.Key == game.Key && g.Id != game.Id.PrimaryId,
                cancellationToken);
        }
        else
        {
            isUniqueKey = await _gameRepository.NotExistsAsync(
                g => g.Key == game.Key && g.ProductId != game.Id.SecondaryId,
                cancellationToken);
        }

        return isUniqueKey;
    }
}