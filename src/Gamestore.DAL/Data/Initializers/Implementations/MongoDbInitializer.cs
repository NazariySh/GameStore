using Gamestore.DAL.Data.Initializers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations;

public class MongoDbInitializer : IDbInitializer
{
    private readonly IEnumerable<IMongoDataUpdater> _dataUpdaters;
    private readonly ILogger<MongoDbInitializer> _logger;

    public MongoDbInitializer(
        IEnumerable<IMongoDataUpdater> dataUpdaters,
        ILogger<MongoDbInitializer> logger)
    {
        _dataUpdaters = dataUpdaters;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting MongoDB initialization");

        await UpdateDataAsync(cancellationToken);

        _logger.LogInformation("MongoDB initialization completed successfully");
    }

    private async Task UpdateDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var dataUpdater in _dataUpdaters.OrderBy(x => x.Order))
            {
                await dataUpdater.UpdateAsync(cancellationToken);
            }

            _logger.LogInformation("Mongo Data update completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating data");
            throw new InvalidOperationException("An error occurred while updating data.", ex);
        }
    }
}