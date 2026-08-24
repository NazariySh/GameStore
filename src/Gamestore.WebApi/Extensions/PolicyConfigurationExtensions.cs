using Gamestore.BLL.Interfaces.Auth;
using Gamestore.Domain.Constants;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Services.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Gamestore.WebApi.Extensions;

public static class PolicyConfigurationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAccessService, AccessService>();

        services.AddAuthorizationBuilder()
            .AddPolicy(CustomPolicyNames.CanManageUsers, policy =>
            {
                policy.RequirePermission(Permissions.ManageUsers);
            })
            .AddPolicy(CustomPolicyNames.CanManageRoles, policy =>
            {
                policy.RequirePermission(Permissions.ManageRoles);
            })
            .AddPolicy(CustomPolicyNames.CanManageGames, policy =>
            {
                policy.RequirePermission(Permissions.ManageGames);
            })
            .AddPolicy(CustomPolicyNames.CanManageGenres, policy =>
            {
                policy.RequirePermission(Permissions.ManageGenres);
            })
            .AddPolicy(CustomPolicyNames.CanManagePlatforms, policy =>
            {
                policy.RequirePermission(Permissions.ManagePlatforms);
            })
            .AddPolicy(CustomPolicyNames.CanManagePublishers, policy =>
            {
                policy.RequirePermission(Permissions.ManagePublishers);
            })
            .AddPolicy(CustomPolicyNames.CanEditOrders, policy =>
            {
                policy.RequirePermission(Permissions.EditOrders);
            })
            .AddPolicy(CustomPolicyNames.CanViewOrdersHistory, policy =>
            {
                policy.RequirePermission(Permissions.ViewOrdersHistory);
            })
            .AddPolicy(CustomPolicyNames.CanChangeOrderStatusToShipped, policy =>
            {
                policy.RequirePermission(Permissions.ChangeOrderStatusToShipped);
            })
            .AddPolicy(CustomPolicyNames.CanManageGameComments, policy =>
            {
                policy.RequirePermission(Permissions.ManageGameComments);
            })
            .AddPolicy(CustomPolicyNames.CanBanUserFromCommenting, policy =>
            {
                policy.RequirePermission(Permissions.BanUserFromCommenting);
            })
            .AddPolicy(CustomPolicyNames.CanCommentGames, policy =>
            {
                policy.RequirePermission(Permissions.CommentGames);
            })
            .AddPolicy(CustomPolicyNames.CanManageUserProfile, policy =>
            {
                policy.RequirePermission(Permissions.ManageUserProfile);
            })
            .AddPolicy(CustomPolicyNames.CanBuyGames, policy =>
            {
                policy.RequirePermission(Permissions.BuyGames);
            });

        return services;
    }

    private static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, params string[] permissions)
    {
        return builder.RequireClaim(CustomClaimTypes.Permission, permissions);
    }
}