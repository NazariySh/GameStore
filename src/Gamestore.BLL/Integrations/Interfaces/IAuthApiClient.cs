using Gamestore.BLL.DTOs.Auth;

namespace Gamestore.BLL.Integrations.Interfaces;

public interface IAuthApiClient
{
    Task<AuthResponseDto> LoginAsync(AuthRequestDto authRequest, CancellationToken cancellationToken = default);
}