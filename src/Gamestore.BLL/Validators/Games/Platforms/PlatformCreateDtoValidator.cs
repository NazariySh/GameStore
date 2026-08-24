using FluentValidation;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Platforms;

public class PlatformCreateDtoValidator : AbstractValidator<PlatformCreateDto>
{
    private readonly IRepository<Platform> _platformRepository;

    public PlatformCreateDtoValidator(
        IRepository<Platform> platformRepository,
        IValidator<PlatformCreateUpdateDto> baseValidator)
    {
        _platformRepository = platformRepository;

        Include(baseValidator);

        RuleFor(x => x.Type)
            .MustAsync(BeUniqueTypeAsync)
            .WithMessage("Platform with this type already exists.");
    }

    private Task<bool> BeUniqueTypeAsync(string type, CancellationToken cancellationToken)
    {
        return _platformRepository.NotExistsAsync(
            p => p.Type == type,
            cancellationToken);
    }
}