using Gamestore.BLL.DTOs.Users;

namespace Gamestore.BLL.Interfaces.Users;

public interface IUserBanService
{
    Task<bool> IsUserBannedAsync(string userName, CancellationToken cancellationToken = default);

    Task BanUserAsync(BanUserRequest request, CancellationToken cancellationToken = default);
}