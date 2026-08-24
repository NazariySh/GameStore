using Gamestore.Domain.Entities;

namespace Gamestore.DAL.Repositories.Interfaces;

public interface ISqlRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IBaseEntity;