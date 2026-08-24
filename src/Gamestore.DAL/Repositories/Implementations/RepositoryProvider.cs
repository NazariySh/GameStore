using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Gamestore.DAL.Repositories.Implementations;

public class RepositoryProvider : IRepositoryProvider
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRepository<TEntity> GetGeneric<TEntity>()
        where TEntity : class, IBaseEntity
    {
        return GetRepositoryOrThrow<IRepository<TEntity>>();
    }

    public TRepository Get<TRepository>()
        where TRepository : IRepository
    {
        return GetRepositoryOrThrow<TRepository>();
    }

    private TRepository GetRepositoryOrThrow<TRepository>()
        where TRepository : IRepository
    {
        return _serviceProvider.GetRequiredService<TRepository>();
    }
}