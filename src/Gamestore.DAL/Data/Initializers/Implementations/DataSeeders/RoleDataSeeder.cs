using System.Security.Claims;
using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class RoleDataSeeder : BaseDataSeeder<Role, RoleDataSeeder.RoleModel>
{
    private readonly RoleManager<Role> _roleManager;

    public RoleDataSeeder(
        IUnitOfWork unitOfWork,
        RoleManager<Role> roleManager,
        ISeedDataExtractor dataExtractor,
        ILogger<RoleDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
        _roleManager = roleManager;
    }

    public override int Order => 1;

    protected override string FileName => "roles";

    protected override async Task AddEntitiesAsync(ICollection<RoleModel> models, CancellationToken cancellationToken)
    {
        foreach (var model in models)
        {
            var role = MapToRole(model);

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                await AddRolePermissionsAsync(role, model.Permissions);
            }
        }
    }

    private async Task AddRolePermissionsAsync(Role role, ICollection<string> permissions)
    {
        foreach (var permission in permissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
        }
    }

    private static Role MapToRole(RoleModel role)
    {
        return new Role
        {
            Id = role.Id,
            Name = role.Name,
        };
    }

    public sealed record RoleModel(
        Guid Id,
        string Name,
        ICollection<string> Permissions);
}