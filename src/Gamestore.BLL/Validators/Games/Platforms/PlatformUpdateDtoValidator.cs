using FluentValidation;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Platforms;

public class PlatformUpdateDtoValidator : AbstractValidator<PlatformUpdateDto>
{
    private readonly IRepository<Platform> _platformRepository;

    public PlatformUpdateDtoValidator(
        IRepository<Platform> platformRepository,
        IValidator<PlatformCreateUpdateDto> baseValidator)
    {
        _platformRepository = platformRepository;

        Include(baseValidator);

        RuleFor(x => x)
            .MustAsync(BeUniqueTypeAsync)
            .WithMessage("Platform with this type already exists.")
            .OverridePropertyName(x => x.Type);
    }

    private Task<bool> BeUniqueTypeAsync(PlatformUpdateDto platform, CancellationToken cancellationToken)
    {
        return _platformRepository.NotExistsAsync(
            p => p.Type == platform.Type && p.Id != platform.Id,
            cancellationToken);
    }
}