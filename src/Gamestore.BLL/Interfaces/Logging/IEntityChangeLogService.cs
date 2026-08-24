using Gamestore.BLL.DTOs.Logging;
using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Interfaces.Logging;

public interface IEntityChangeLogService
{
    Task LogChangeAsync<T>(ChangeLogDto<T> changeLog, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;
}