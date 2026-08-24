using System.Security.Claims;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Interfaces.Auth;
using Gamestore.Domain.Shared;
using Gamestore.WebApi.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Gamestore.WebApi.Services.Auth;

public class AccessService : IAccessService
{
    private readonly Dictionary<string, string> _policies = PagePolicy.AllPolicies;
    private readonly IAuthorizationService _authorizationService;

    public AccessService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<bool> CheckUserAccessAsync(ClaimsPrincipal user, CheckAccessRequest request, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(user);
        Guard.AgainstNullOrWhiteSpace(request.TargetPage);

        if (!_policies.TryGetValue(request.TargetPage, out var policy))
        {
            return false;
        }

        if (policy is CustomPolicyNames.ReadOnlyPolicy)
        {
            return true;
        }

        var authorizeResult = await _authorizationService.AuthorizeAsync(user, policy);
        return authorizeResult.Succeeded;
    }
}