using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Data.Implementations;

public class CategoryRepositoryFacade : RepositoryFacade<Genre>
{
    private readonly ISqlRepository<Genre> _sqlRepository;
    private readonly IMongoRepository<Genre> _mongoRepository;

    public CategoryRepositoryFacade(
        ISqlRepository<Genre> sqlRepository,
        IMongoRepository<Genre> mongoRepository)
        : base(sqlRepository, mongoRepository)
    {
        _sqlRepository = sqlRepository;
        _mongoRepository = mongoRepository;
    }

    public override async Task<Genre> AddAsync(Genre entity, CancellationToken cancellationToken = default)
    {
        await UpdateParentGenreAsync(entity, cancellationToken);

        return await _sqlRepository.AddAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(Genre entity, CancellationToken cancellationToken = default)
    {
        var categoryExistsInSql = await ExistsInSqlAsync(entity.Id, cancellationToken);
        var categoryExistsInMongo = await ExistsInMongoAsync(entity.ObjectId, cancellationToken);

        if (categoryExistsInSql)
        {
            await UpdateParentGenreAsync(entity, cancellationToken);

            await _sqlRepository.UpdateAsync(entity, cancellationToken);
        }
        else if (categoryExistsInMongo)
        {
            await UpdateParentGenreAsync(entity, cancellationToken);

            await _sqlRepository.AddAsync(entity, cancellationToken);
        }
    }

    protected override ICollection<Genre> MergeCollections(
        IEnumerable<Genre> firstCollection,
        IEnumerable<Genre> secondCollection)
    {
        return firstCollection.Concat(secondCollection)
            .DistinctBy(c => c.Name)
            .ToList();
    }

    private async Task UpdateParentGenreAsync(Genre genre, CancellationToken cancellationToken)
    {
        var parentGenreId = genre.ParentGenreId;
        var parentCategoryId = genre.ParentCategoryId;

        if (!parentGenreId.HasValue && !parentCategoryId.HasValue)
        {
            return;
        }

        if (parentGenreId.HasValue && parentGenreId.Value != Guid.Empty)
        {
            return;
        }

        if (!parentCategoryId.HasValue)
        {
            return;
        }

        genre.ParentGenreId = await AddParentCategoryAsync(parentCategoryId.Value, cancellationToken);
    }

    private async Task<Guid?> AddParentCategoryAsync(int parentCategoryId, CancellationToken cancellationToken)
    {
        var parentCategory = await _mongoRepository.GetAsync(
            e => e.CategoryId == parentCategoryId,
            cancellationToken: cancellationToken);

        if (parentCategory is null)
        {
            return null;
        }

        if (parentCategory.Id == Guid.Empty)
        {
            parentCategory.Id = Guid.NewGuid();
        }

        await _sqlRepository.AddAsync(parentCategory, cancellationToken);

        return parentCategory.Id;
    }
}