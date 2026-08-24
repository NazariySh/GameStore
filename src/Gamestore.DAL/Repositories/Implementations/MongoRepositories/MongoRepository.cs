using System.Linq.Expressions;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore.Query;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public abstract class MongoRepository<TEntity> : IMongoRepository<TEntity>
    where TEntity : BaseEntity
{
    protected MongoRepository(NorthwindMongoDbContext dbContext)
    {
        DbContext = dbContext;
        Collection = dbContext.GetCollection<TEntity>();
    }

    protected NorthwindMongoDbContext DbContext { get; }

    protected IMongoCollection<TEntity> Collection { get; }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var insert = new BsonDocument
        {
            { "insert", Collection.CollectionNamespace.CollectionName },
            { "documents", new BsonArray { entity.ToBsonDocument() } },
        };

        await DbContext.Database.RunCommandAsync<BsonDocument>(insert, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await Collection.InsertManyAsync(entities, cancellationToken: cancellationToken);
    }

    public abstract Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    public virtual async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var delete = new BsonDocument
        {
            { "delete", Collection.CollectionNamespace.CollectionName },
            {
                "deletes",
                new BsonArray
                {
                    new BsonDocument
                    {
                        { "q", new BsonDocument { { "_id", entity.ObjectId } } },
                        { "limit", 1 },
                    },
                }
            },
        };

        await DbContext.Database.RunCommandAsync<BsonDocument>(delete, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(predicate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TProjection?> GetAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(predicate)
            .ProjectToType<TProjection>()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TProjection?> GetFirstSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(predicate)
            .Select(selector)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(predicate)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TProjection?> GetSingleAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(predicate)
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
        return await GetQueryable(predicate)
            .ProjectToType<TProjection>()
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        return await queryModifier(GetQueryable())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await queryModifier(GetQueryable())
            .ProjectToType<TProjection>()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await GetQueryable()
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await GetQueryable(predicate)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TProjection>> GetAllSelectedAsync<TProjection>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryModifier,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
        where TProjection : IEquatable<TProjection>
    {
        return await queryModifier(GetQueryable())
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
        return GetQueryable(predicate).CountAsync(cancellationToken);
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

    private IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = Collection.AsQueryable();

        var globalFilter = SetGlobalFilter();
        if (globalFilter is not null)
        {
            query = query.Where(globalFilter);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query;
    }
}