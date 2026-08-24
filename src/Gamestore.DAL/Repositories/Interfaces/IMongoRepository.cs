using Gamestore.Domain.Entities;

namespace Gamestore.DAL.Repositories.Interfaces;

public interface IMongoRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IBaseEntity;