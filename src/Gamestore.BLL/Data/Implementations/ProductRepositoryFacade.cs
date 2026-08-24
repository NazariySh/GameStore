using System.Linq.Expressions;
using Gamestore.BLL.Data.Interfaces;
using Gamestore.DAL.Repositories.Implementations.MongoRepositories;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Data.Implementations;

public class ProductRepositoryFacade : RepositoryFacade<Game>, IProductRepositoryFacade
{
    private readonly GameRepository _sqlRepository;
    private readonly ProductMongoRepository _mongoRepository;
    private readonly ISqlRepository<Publisher> _sqlPublisherRepository;
    private readonly IMongoRepository<Publisher> _mongoPublisherRepository;
    private readonly ISqlRepository<Genre> _sqlGenreRepository;
    private readonly IMongoRepository<Genre> _mongoGenreRepository;

    public ProductRepositoryFacade(
        GameRepository sqlRepository,
        ProductMongoRepository mongoRepository,
        ISqlRepository<Publisher> sqlPublisherRepository,
        IMongoRepository<Publisher> mongoPublisherRepository,
        ISqlRepository<Genre> sqlGenreRepository,
        IMongoRepository<Genre> mongoGenreRepository)
        : base(sqlRepository, mongoRepository)
    {
        _sqlRepository = sqlRepository;
        _mongoRepository = mongoRepository;
        _sqlPublisherRepository = sqlPublisherRepository;
        _mongoPublisherRepository = mongoPublisherRepository;
        _sqlGenreRepository = sqlGenreRepository;
        _mongoGenreRepository = mongoGenreRepository;
    }

    public override async Task<Game> AddAsync(Game entity, CancellationToken cancellationToken = default)
    {
        await UpdatePublisherAsync(entity, cancellationToken);
        await UpdateGenresAsync(entity, cancellationToken);

        return await _sqlRepository.AddAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        var productExistsInSql = await ExistsInSqlAsync(entity.Id, cancellationToken);
        var productExistsInMongo = await ExistsInMongoAsync(entity.ObjectId, cancellationToken);

        if (productExistsInSql)
        {
            await UpdatePublisherAsync(entity, cancellationToken);
            await UpdateGenresAsync(entity, cancellationToken);

            await _sqlRepository.UpdateAsync(entity, cancellationToken);
        }
        else if (productExistsInMongo)
        {
            await UpdatePublisherAsync(entity, cancellationToken);
            await UpdateGenresAsync(entity, cancellationToken);

            entity.SupplierId = null;
            entity.CategoryId = null;

            await _sqlRepository.AddAsync(entity, cancellationToken);
        }
    }

    public async Task SoftDeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        var productExistsInSql = await ProductExistsInSqlAsync(game.Key, cancellationToken);
        var productExistsInMongo = await ProductExistsInMongoAsync(game.Key, cancellationToken);

        if (productExistsInSql)
        {
            await _sqlRepository.SoftDeleteAsync(game, cancellationToken);
        }

        if (productExistsInMongo)
        {
            await _mongoRepository.SoftDeleteAsync(game, cancellationToken);
        }
    }

    public async Task UpdateUnitInStockAsync(Game game, CancellationToken cancellationToken = default)
    {
        var productExistsInSql = await ProductExistsInSqlAsync(game.Key, cancellationToken);
        var productExistsInMongo = await ProductExistsInMongoAsync(game.Key, cancellationToken);

        if (productExistsInSql)
        {
            await _sqlRepository.UpdateUnitInStockAsync(game, cancellationToken);
        }

        if (productExistsInMongo)
        {
            await _mongoRepository.UpdateUnitInStockAsync(game, cancellationToken);
        }
    }

    public async Task UpdateViewCountAsync(Game game, CancellationToken cancellationToken = default)
    {
        var productExistsInSql = await ProductExistsInSqlAsync(game.Key, cancellationToken);
        var productExistsInMongo = await ProductExistsInMongoAsync(game.Key, cancellationToken);

        if (productExistsInSql)
        {
            await _sqlRepository.UpdateViewCountAsync(game, cancellationToken);
        }

        if (productExistsInMongo)
        {
            await _mongoRepository.UpdateViewCountAsync(game, cancellationToken);
        }
    }

    public async Task UpdateCommentCountAsync(Game game, CancellationToken cancellationToken = default)
    {
        var productExistsInSql = await ProductExistsInSqlAsync(game.Key, cancellationToken);
        var productExistsInMongo = await ProductExistsInMongoAsync(game.Key, cancellationToken);

        if (productExistsInSql)
        {
            await _sqlRepository.UpdateCommentCountAsync(game, cancellationToken);
        }

        if (productExistsInMongo)
        {
            await _mongoRepository.UpdateCommentCountAsync(game, cancellationToken);
        }
    }

    public async Task RecalculateUnitInStockCountAsync(ICollection<string> productKeys, CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.GetAllAsync(g => productKeys.Contains(g.Key), cancellationToken);
        var mongoTask = _mongoRepository.GetAllAsync(g => productKeys.Contains(g.Key), cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        var sqlProducts = sqlTask.Result.ToDictionary(p => p.Key);
        var mongoProducts = mongoTask.Result.ToDictionary(p => p.Key);

        foreach (var productKey in productKeys)
        {
            if (!sqlProducts.TryGetValue(productKey, out var sqlProduct) ||
                !mongoProducts.TryGetValue(productKey, out var mongoProduct))
            {
                continue;
            }

            var maxUnitInStock = Math.Max(sqlProduct.UnitInStock, mongoProduct.UnitInStock);

            sqlProduct.UnitInStock = maxUnitInStock;
            mongoProduct.UnitInStock = maxUnitInStock;

            await _sqlRepository.UpdateUnitInStockAsync(sqlProduct, cancellationToken);
            await _mongoRepository.UpdateUnitInStockAsync(mongoProduct, cancellationToken);
        }
    }

    public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.GetAllSelectedAsync(x => x.Key, cancellationToken);
        var mongoTask = _mongoRepository.GetAllSelectedAsync(x => x.Key, cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result.Union(mongoTask.Result).Count();
    }

    public override async Task<int> CountAsync(Expression<Func<Game, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.GetAllSelectedAsync(predicate, x => x.Key, cancellationToken);
        var mongoTask = _mongoRepository.GetAllSelectedAsync(predicate, x => x.Key, cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result.Union(mongoTask.Result).Count();
    }

    public override async Task<int> CountAsync(
        Func<IQueryable<Game>, IQueryable<Game>> queryModifier,
        CancellationToken cancellationToken = default)
    {
        var sqlTask = _sqlRepository.GetAllSelectedAsync(queryModifier, x => x.Key, cancellationToken);
        var mongoTask = _mongoRepository.GetAllSelectedAsync(queryModifier, x => x.Key, cancellationToken);

        await Task.WhenAll(sqlTask, mongoTask);

        return sqlTask.Result.Union(mongoTask.Result).Count();
    }

    protected override ICollection<Game> MergeCollections(
        IEnumerable<Game> firstCollection,
        IEnumerable<Game> secondCollection)
    {
        return firstCollection.Concat(secondCollection)
            .DistinctBy(p => p.Key)
            .ToList();
    }

    private async Task UpdatePublisherAsync(Game entity, CancellationToken cancellationToken)
    {
        var publisherId = entity.PublisherId;
        var supplierId = entity.SupplierId;

        if (publisherId == Guid.Empty && !supplierId.HasValue)
        {
            return;
        }

        if (publisherId != Guid.Empty)
        {
            return;
        }

        if (!supplierId.HasValue)
        {
            return;
        }

        var newPublisherId = await AddSupplierAsync(supplierId.Value, cancellationToken);
        if (newPublisherId.HasValue)
        {
            entity.PublisherId = newPublisherId.Value;
        }
    }

    private async Task UpdateGenresAsync(Game entity, CancellationToken cancellationToken)
    {
        var genreIds = entity.GameGenres
            .Where(gg => gg.GenreId != Guid.Empty)
            .Select(gg => gg.GenreId)
            .ToList();

        var categoryIds = entity.GameGenres
            .Where(gg => gg.CategoryId.HasValue)
            .Select(gg => gg.CategoryId!.Value)
            .ToList();

        var newGenreIds = await AddCategoriesAsync(categoryIds, cancellationToken);
        genreIds.AddRange(newGenreIds);

        entity.GameGenres = genreIds
            .Select(genreId => new GameGenre { GameId = entity.Id, GenreId = genreId })
            .ToList();
    }

    private async Task<List<Guid>> AddCategoriesAsync(List<int> categoryIds, CancellationToken cancellationToken)
    {
        var genreIds = new List<Guid>();

        var categories = await _mongoGenreRepository.GetAllAsync(
            g => g.CategoryId.HasValue && categoryIds.Contains(g.CategoryId.Value),
            cancellationToken);

        foreach (var category in categories)
        {
            if (category.Id == Guid.Empty)
            {
                category.Id = Guid.NewGuid();
            }

            await _sqlGenreRepository.AddAsync(category, cancellationToken);

            genreIds.Add(category.Id);
        }

        return genreIds;
    }

    private async Task<Guid?> AddSupplierAsync(int supplierId, CancellationToken cancellationToken)
    {
        var supplier = await _mongoPublisherRepository.GetAsync(
            s => s.SupplierId == supplierId,
            cancellationToken: cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        if (supplier.Id == Guid.Empty)
        {
            supplier.Id = Guid.NewGuid();
        }

        await _sqlPublisherRepository.AddAsync(supplier, cancellationToken);

        return supplier.Id;
    }

    private Task<bool> ProductExistsInSqlAsync(string gameKey, CancellationToken cancellationToken)
    {
        return _sqlRepository.ExistsAsync(g => g.Key == gameKey, cancellationToken);
    }

    private Task<bool> ProductExistsInMongoAsync(string gameKey, CancellationToken cancellationToken)
    {
        return _mongoRepository.ExistsAsync(g => g.Key == gameKey, cancellationToken);
    }
}