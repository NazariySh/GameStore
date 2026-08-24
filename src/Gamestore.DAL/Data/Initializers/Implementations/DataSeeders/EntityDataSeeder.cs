using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class EntityDataSeeder<TEntity> : BaseDataSeeder<TEntity, TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<TEntity> _repository;

    public EntityDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<EntityDataSeeder<TEntity>> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.Repositories.Get<ISqlRepository<TEntity>>();
    }

    public override int Order => 0;

    protected override string FileName => $"{typeof(TEntity).Name.ToLowerInvariant()}s";

    protected override async Task AddEntitiesAsync(ICollection<TEntity> models, CancellationToken cancellationToken)
    {
        await _repository.AddRangeAsync(models, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}