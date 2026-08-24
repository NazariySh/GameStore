using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using MongoDB.Bson;

namespace Gamestore.BLL.Data.Interfaces;

public interface IRepositoryFacade<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<bool> ExistsInSqlAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsInMongoAsync(ObjectId id, CancellationToken cancellationToken = default);
}