using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class UserDataSeeder : BaseDataSeeder<User, UserDataSeeder.UserModel>
{
    private readonly UserManager<User> _userManager;

    public UserDataSeeder(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ISeedDataExtractor dataExtractor,
        ILogger<UserDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
        _userManager = userManager;
    }

    public override int Order => 2;

    protected override string FileName => "users";

    protected override async Task AddEntitiesAsync(ICollection<UserModel> models, CancellationToken cancellationToken)
    {
        foreach (var model in models)
        {
            var user = MapToUser(model);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, model.Roles);
            }
        }
    }

    private static User MapToUser(UserModel user)
    {
        return new User
        {
            Id = user.Id,
            UserName = user.UserName,
        };
    }

    public sealed record UserModel(
        Guid Id,
        string UserName,
        string Password,
        ICollection<string> Roles);
}