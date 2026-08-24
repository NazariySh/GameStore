using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Enums;

namespace Gamestore.BLL.Tests.TestData.Users;

public static class RoleTestData
{
    public static List<Role> GetRoles()
    {
        return
        [
            new Role
            {
                Id = Guid.Parse("e2a4c9f1-53b7-4b1a-8a72-5f3e7adc9123"),
                Name = nameof(RoleType.Administrator),
                NormalizedName = nameof(RoleType.Administrator).ToUpperInvariant(),
                RoleClaims =
                [
                    new RoleClaim { Id = 1, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageUsers },
                    new RoleClaim { Id = 2, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageRoles },
                    new RoleClaim { Id = 3, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ViewDeletedGames },
                    new RoleClaim { Id = 4, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageCommentsForDeletedGames },
                    new RoleClaim { Id = 5, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.EditDeletedGames },
                ],
            },
            new Role
            {
                Id = Guid.Parse("4c7b2e65-9f32-46c5-b182-7a4d9e2c45f6"),
                Name = nameof(RoleType.Manager),
                NormalizedName = nameof(RoleType.Manager).ToUpperInvariant(),
                RoleClaims =
                [
                    new RoleClaim { Id = 6, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageGames },
                    new RoleClaim { Id = 7, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageGenres },
                    new RoleClaim { Id = 8, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManagePlatforms },
                    new RoleClaim { Id = 9, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManagePublishers },
                    new RoleClaim { Id = 10, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.EditOrders },
                    new RoleClaim { Id = 11, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ViewOrdersHistory },
                    new RoleClaim { Id = 12, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ChangeOrderStatusToShipped },
                ],
            },
            new Role
            {
                Id = Guid.Parse("12d8a7b9-65de-4f4c-93a1-8b1f27e3c678"),
                Name = nameof(RoleType.Moderator),
                NormalizedName = nameof(RoleType.Moderator).ToUpperInvariant(),
                RoleClaims =
                [
                    new RoleClaim { Id = 13, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageGameComments },
                    new RoleClaim { Id = 14, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.BanUserFromCommenting },
                ],
            },
            new Role
            {
                Id = Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                Name = nameof(RoleType.User),
                NormalizedName = nameof(RoleType.User).ToUpperInvariant(),
                RoleClaims =
                [
                    new RoleClaim { Id = 15, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.CommentGames },
                    new RoleClaim { Id = 16, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.ManageUserProfile },
                    new RoleClaim { Id = 17, ClaimType = CustomClaimTypes.Permission, ClaimValue = Permissions.BuyGames },
                ],
            },
        ];
    }

    public static List<Role> GetRoles(ICollection<Guid> ids)
    {
        return GetRoles().Where(r => ids.Contains(r.Id)).ToList();
    }

    public static Role GetRole()
    {
        return GetRoles()[0];
    }

    public static CreateRoleRequest GetCreateRequest()
    {
        return new CreateRoleRequest
        {
            Role = new RoleCreateDto
            {
                Name = nameof(RoleType.Administrator),
            },
            Permissions = new List<string> { Permissions.ManageUsers, Permissions.ManageRoles },
        };
    }

    public static CreateRoleRequest GetInvalidCreateRequest()
    {
        return new CreateRoleRequest
        {
            Role = new RoleCreateDto
            {
                Name = string.Empty,
            },
            Permissions = new List<string>(),
        };
    }

    public static UpdateRoleRequest GetUpdateRequest(Guid? id = null)
    {
        return new UpdateRoleRequest
        {
            Role = new RoleUpdateDto
            {
                Id = id ?? Guid.NewGuid(),
                Name = nameof(RoleType.Manager),
            },
            Permissions = new List<string> { Permissions.ManageGames, Permissions.ManageGenres },
        };
    }

    public static UpdateRoleRequest GetInvalidUpdateRequest(Guid? id = null)
    {
        return new UpdateRoleRequest
        {
            Role = new RoleUpdateDto
            {
                Id = id ?? Guid.NewGuid(),
                Name = string.Empty,
            },
            Permissions = new List<string>(),
        };
    }
}