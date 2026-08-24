using FluentValidation;
using Gamestore.BLL.DTOs.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users;

public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
    private readonly IRepository<User> _userRepository;

    public UserCreateDtoValidator(
        IRepository<User> userRepository,
        IValidator<UserCreateUpdateDto> baseValidator)
    {
        _userRepository = userRepository;

        Include(baseValidator);

        RuleFor(x => x.Name)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("User with this name already exists.");
    }

    private Task<bool> BeUniqueNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();
        return _userRepository.NotExistsAsync(
            u => u.NormalizedUserName == normalizedName,
            cancellationToken);
    }
}