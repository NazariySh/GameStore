using Gamestore.Domain.Entities;

namespace Gamestore.DAL.Repositories.Interfaces;

public interface IRepositoryProvider
{
    IRepository<TEntity> GetGeneric<TEntity>()
        where TEntity : class, IBaseEntity;

    TRepository Get<TRepository>()
        where TRepository : IRepository;
}