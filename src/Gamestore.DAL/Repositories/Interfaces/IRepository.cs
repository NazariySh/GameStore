using System.Linq.Expressions;
using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace Gamestore.DAL.Repositories.Interfaces;

public interface IRepository;

public interface IRepository<TEntity> : IRepository
    where TEntity : class, IBaseEntity
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default);

    Task<TProjection?> GetAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TProjection?> GetFirstSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default);

    Task<TProjection?> GetSingleAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default);

    Task<TProjection?> GetByIdAsync<TProjection>(Guid id, CancellationToken cancellationToken = default);

    Task<ICollection<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>;

    Task<ICollection<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>;

    Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>;

    Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>;

    Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>;

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> NotExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default);
}