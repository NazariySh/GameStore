using System.Linq.Expressions;
using Gamestore.BLL.Data.Interfaces;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using MongoDB.Bson;

namespace Gamestore.BLL.Data.Implementations;

public class RepositoryFacade<TEntity> : IRepositoryFacade<TEntity>
    where TEntity : BaseEntity
{
    private readonly ISqlRepository<TEntity> _sqlRepository;
    private readonly IMongoRepository<TEntity> _mongoRepository;

    public RepositoryFacade(
        ISqlRepository<TEntity> sqlRepository,
        IMongoRepository<TEntity> mongoRepository)
    {
        _sqlRepository = sqlRepository;
        _mongoRepository = mongoRepository;
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return await _sqlRepository.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _sqlRepository.AddRangeAsync(entities, cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityExistsInSql = await ExistsInSqlAsync(entity.Id, cancellationToken);
        var entityExistsInMongo = await ExistsInMongoAsync(entity.ObjectId, cancellationToken);

        if (entityExistsInSql)
        {
            await _sqlRepository.UpdateAsync(entity, cancellationToken);
        }
        else if (entityExistsInMongo)
        {
            await _sqlRepository.AddAsync(entity, cancellationToken);
        }
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityExistsInSql = await ExistsInSqlAsync(entity.Id, cancellationToken);
        var entityExistsInMongo = await ExistsInMongoAsync(entity.ObjectId, cancellationToken);

        if (entityExistsInSql)
        {
            await _sqlRepository.RemoveAsync(entity, cancellationToken);
        }

        if (entityExistsInMongo)
        {
            await _mongoRepository.RemoveAsync(entity, cancellationToken);
        }
    }

    public Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetAsync(predicate, include, cancellationToken),
            _mongoRepository.GetAsync(predicate, include, cancellationToken));
    }

    public Task<TProjection?> GetAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetAsync<TProjection>(predicate, cancellationToken),
            _mongoRepository.GetAsync<TProjection>(predicate, cancellationToken));
    }

    public Task<TProjection?> GetFirstSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetFirstSelectedAsync(predicate, selector, cancellationToken),
            _mongoRepository.GetFirstSelectedAsync(predicate, selector, cancellationToken));
    }

    public Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetSingleAsync(predicate, include, cancellationToken),
            _mongoRepository.GetSingleAsync(predicate, include, cancellationToken));
    }

    public Task<TProjection?> GetSingleAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetSingleAsync<TProjection>(predicate, cancellationToken),
            _mongoRepository.GetSingleAsync<TProjection>(predicate, cancellationToken));
    }

    public Task<TEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetByIdAsync(id, include, cancellationToken),
            _mongoRepository.GetByIdAsync(id, include, cancellationToken));
    }

    public Task<TProjection?> GetByIdAsync<TProjection>(Guid id, CancellationToken cancellationToken = default)
    {
        return GetFromSqlOrMongoAsync(
            _sqlRepository.GetByIdAsync<TProjection>(id, cancellationToken),
            _mongoRepository.GetByIdAsync<TProjection>(id, cancellationToken));
    }

    public Task<ICollection<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllAsync(predicate, cancellationToken),
            _mongoRepository.GetAllAsync(predicate, cancellationToken));
    }

    public Task<ICollection<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllAsync(queryModifier, asNoTracking, cancellationToken),
            _mongoRepository.GetAllAsync(queryModifier, asNoTracking, cancellationToken));
    }

    public Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllAsync<TProjection>(predicate, cancellationToken),
            _mongoRepository.GetAllAsync<TProjection>(predicate, cancellationToken));
    }

    public Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllAsync<TProjection>(queryModifier, cancellationToken),
            _mongoRepository.GetAllAsync<TProjection>(queryModifier, cancellationToken));
    }

    public Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllSelectedAsync(selector, cancellationToken),
            _mongoRepository.GetAllSelectedAsync(selector, cancellationToken));
    }

    public Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllSelectedAsync(predicate, selector, cancellationToken),
            _mongoRepository.GetAllSelectedAsync(predicate, selector, cancellationToken));
    }

    public Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return GetFromSqlAndMongoAsync(
            _sqlRepository.GetAllSelectedAsync(queryModifier, selector, cancellationToken),
            _mongoRepository.GetAllSelectedAsync(queryModifier, selector, cancellationToken));
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var existsInSql = await _sqlRepository.ExistsAsync(predicate, cancellationToken);
        if (existsInSql)
        {
            return true;
        }

        var existsInMongo = await _mongoRepository.ExistsAsync(predicate, cancellationToken);
        return existsInMongo;
    }

    public async Task<bool> NotExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return !await ExistsAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        var anyInSql = await _sqlRepository.AnyAsync(cancellationToken);
        if (anyInSql)
        {
            return true;
        }

        var anyInMongo = await _mongoRepository.AnyAsync(cancellationToken);
        return anyInMongo;
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.CountAsync(cancellationToken);
        var mongoTask = _mongoRepository.CountAsync(cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result + mongoTask.Result;
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.CountAsync(predicate, cancellationToken);
        var mongoTask = _mongoRepository.CountAsync(predicate, cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result + mongoTask.Result;
    }

    public virtual async Task<int> CountAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.CountAsync(queryModifier, cancellationToken);
        var mongoTask = _mongoRepository.CountAsync(queryModifier, cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result + mongoTask.Result;
    }

    public Task<bool> ExistsInSqlAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _sqlRepository.ExistsAsync(e => e.Id == id, cancellationToken);
    }

    public Task<bool> ExistsInMongoAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        return _mongoRepository.ExistsAsync(e => e.ObjectId == id, cancellationToken);
    }

    protected virtual ICollection<TEntity> MergeCollections(
        IEnumerable<TEntity> firstCollection,
        IEnumerable<TEntity> secondCollection)
    {
        return firstCollection.Concat(secondCollection).ToList();
    }

    private async Task<ICollection<TEntity>> GetFromSqlAndMongoAsync(
        Task<ICollection<TEntity>> sqlTask,
        Task<ICollection<TEntity>> mongoTask)
    {
        await Task.WhenAll(sqlTask, mongoTask);

        return MergeCollections(sqlTask.Result, mongoTask.Result);
    }

    private static async Task<IReadOnlyList<TProjection>> GetFromSqlAndMongoAsync<TProjection>(
        Task<IReadOnlyList<TProjection>> sqlTask,
        Task<IReadOnlyList<TProjection>> mongoTask)
        where TProjection : IEquatable<TProjection>
    {
        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result.Concat(mongoTask.Result)
            .Distinct()
            .ToList();
    }

    private static async Task<TEntity?> GetFromSqlOrMongoAsync(Task<TEntity?> sqlTask, Task<TEntity?> mongoTask)
    {
        var sqlResult = await sqlTask;
        if (sqlResult is not null)
        {
            return sqlResult;
        }

        var mongoResult = await mongoTask;
        return mongoResult;
    }

    private static async Task<TProjection?> GetFromSqlOrMongoAsync<TProjection>(Task<TProjection?> sqlTask, Task<TProjection?> mongoTask)
    {
        var sqlResult = await sqlTask;
        if (sqlResult is not null)
        {
            return sqlResult;
        }

        var mongoResult = await mongoTask;
        return mongoResult;
    }
}