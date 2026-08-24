using System.Linq.Expressions;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Gamestore.DAL.Repositories.Implementations.SqlRepositories;

public class Repository<TEntity> : ISqlRepository<TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly DbSet<TEntity> _dbSet;

    public Repository(GamestoreDbContext dbContext)
    {
        DbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    protected GamestoreDbContext DbContext { get; }

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var newEntity = _dbSet.Add(entity).Entity;
        return Task.FromResult(newEntity);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(predicate, include)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TProjection?> GetAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return GetNoTrackingQueryable(predicate)
            .ProjectToType<TProjection>()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TProjection?> GetFirstSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
    {
        return GetNoTrackingQueryable(predicate)
            .Select(selector)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(predicate, include)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<TProjection?> GetSingleAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return GetNoTrackingQueryable(predicate)
            .ProjectToType<TProjection>()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return GetSingleAsync(x => x.Id == id, include, cancellationToken);
    }

    public Task<TProjection?> GetByIdAsync<TProjection>(Guid id, CancellationToken cancellationToken = default)
    {
        return GetSingleAsync<TProjection>(x => x.Id == id, cancellationToken);
    }

    public async Task<ICollection<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await GetNoTrackingQueryable(predicate)
            .ProjectToType<TProjection>()
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking
            ? GetNoTrackingQueryable()
            : GetQueryable();

        return await queryModifier(query)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await queryModifier(GetNoTrackingQueryable())
            .ProjectToType<TProjection>()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await GetNoTrackingQueryable()
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await GetNoTrackingQueryable(predicate)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await queryModifier(GetNoTrackingQueryable())
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return GetQueryable().AnyAsync(predicate, cancellationToken);
    }

    public async Task<bool> NotExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return !await ExistsAsync(predicate, cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return GetQueryable().AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return GetQueryable().CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return GetQueryable().CountAsync(predicate, cancellationToken);
    }

    public Task<int> CountAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default)
    {
        return queryModifier(GetQueryable()).CountAsync(cancellationToken);
    }

    protected virtual Expression<Func<TEntity, bool>>? SetGlobalFilter()
    {
        return null;
    }

    private IQueryable<TEntity> GetNoTrackingQueryable(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        return GetQueryable(predicate, include).AsNoTracking();
    }

    private IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        var query = _dbSet.AsQueryable();

        var globalFilter = SetGlobalFilter();
        if (globalFilter is not null)
        {
            query = query.Where(globalFilter);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (include is not null)
        {
            query = include(query);
        }

        return query;
    }
}