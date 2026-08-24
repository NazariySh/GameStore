using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Entities.Logging;
using MongoDB.Bson;

namespace Gamestore.BLL.Services.Logging;

public class EntityChangeLogService : IEntityChangeLogService
{
    private readonly IMongoRepository<EntityChangeLog> _entityChangeLogRepository;

    public EntityChangeLogService(IMongoRepository<EntityChangeLog> entityChangeLogRepository)
    {
        _entityChangeLogRepository = entityChangeLogRepository;
    }

    public async Task LogChangeAsync<T>(ChangeLogDto<T> changeLog, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        var newChangeLog = new EntityChangeLog
        {
            ObjectId = ObjectId.GenerateNewId(),
            Id = Guid.NewGuid(),
            Action = changeLog.Action,
            EntityType = typeof(T).Name,
            Date = DateTime.UtcNow,
            OldVersion = changeLog.OldVersion?.ToBsonDocument(),
            NewVersion = changeLog.NewVersion?.ToBsonDocument(),
        };

        await _entityChangeLogRepository.AddAsync(newChangeLog, cancellationToken);
    }
}