using FluentValidation;
using Gamestore.BLL.DTOs.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users;

public class BanUserRequestValidator : AbstractValidator<BanUserRequest>
{
    private readonly IRepository<User> _userRepository;

    public BanUserRequestValidator(IRepository<User> userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.User)
            .NotEmpty().WithMessage("User name is required.")
            .MustAsync(BeValidUserNameAsync)
            .WithMessage((_, username) => $"User with name '{username}' does not exist.");

        RuleFor(x => x.Duration)
            .NotNull().WithMessage("Ban duration is required.");
    }

    private Task<bool> BeValidUserNameAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = username.ToUpperInvariant();
        return _userRepository.ExistsAsync(
            u => u.NormalizedUserName == normalizedUsername,
            cancellationToken);
    }
}