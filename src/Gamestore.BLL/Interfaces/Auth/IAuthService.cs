using Gamestore.BLL.DTOs.Auth;

namespace Gamestore.BLL.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginWithExternalAuthAsync(LoginRequest request, CancellationToken cancellationToken = default);
}