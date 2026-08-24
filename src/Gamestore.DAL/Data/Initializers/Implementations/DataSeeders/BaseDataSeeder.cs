using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Data.Initializers.Interfaces;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public abstract class BaseDataSeeder<TEntity, TModel> : ISqlDataSeeder
    where TEntity : class, IBaseEntity
{
    private readonly IRepository<TEntity> _repository;
    private readonly ISeedDataExtractor _dataExtractor;
    private readonly ILogger<BaseDataSeeder<TEntity, TModel>> _logger;

    private readonly string _entityName = $"{typeof(TEntity).Name}s";

    protected BaseDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<BaseDataSeeder<TEntity, TModel>> logger)
    {
        _repository = unitOfWork.Repositories.Get<ISqlRepository<TEntity>>();
        _dataExtractor = dataExtractor;
        _logger = logger;
    }

    public abstract int Order { get; }

    protected abstract string FileName { get; }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {EntityName} from file {FileName}", _entityName, FileName);

        if (await EntitiesExistAsync(cancellationToken))
        {
            _logger.LogInformation("Skipping seeding {EntityName} as they already exist in the database", _entityName);
            return;
        }

        var entities = await GetEntitiesAsync(cancellationToken);

        if (entities.Count == 0)
        {
            _logger.LogWarning("No {EntityName} found in the seed data", _entityName);
            return;
        }

        await AddEntitiesAsync(entities, cancellationToken);

        _logger.LogInformation("Seeded {Count} {EntityName} into the database", entities.Count, _entityName);
    }

    protected abstract Task AddEntitiesAsync(ICollection<TModel> models, CancellationToken cancellationToken);

    private Task<List<TModel>> GetEntitiesAsync(CancellationToken cancellationToken)
    {
        return _dataExtractor.ExtractAsync<TModel>(FileName, cancellationToken);
    }

    private Task<bool> EntitiesExistAsync(CancellationToken cancellationToken)
    {
        return _repository.AnyAsync(cancellationToken);
    }
}