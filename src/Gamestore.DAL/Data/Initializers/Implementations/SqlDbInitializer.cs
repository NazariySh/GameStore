using Gamestore.DAL.Data.Initializers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations;

public class SqlDbInitializer : IDbInitializer
{
    private readonly GamestoreDbContext _dbContext;
    private readonly IEnumerable<ISqlDataSeeder> _dataSeeders;
    private readonly ILogger<SqlDbInitializer> _logger;

    public SqlDbInitializer(
        GamestoreDbContext dbContext,
        IEnumerable<ISqlDataSeeder> dataSeeders,
        ILogger<SqlDbInitializer> logger)
    {
        _dbContext = dbContext;
        _dataSeeders = dataSeeders;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database initialization");

        await ApplyMigrationsAsync(cancellationToken);

        await SeedDataAsync(cancellationToken);

        _logger.LogInformation("Database initialization completed successfully");
    }

    private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var migrations = (await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (migrations.Count > 0)
            {
                await _dbContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("Applied {Count} pending migrations", migrations.Count);
            }
            else
            {
                _logger.LogInformation("No pending migrations to apply");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying database migrations");
            throw new InvalidOperationException("An error occurred while applying database migrations.", ex);
        }
    }

    private async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var dataSeeder in _dataSeeders.OrderBy(x => x.Order))
            {
                await dataSeeder.SeedAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "An error occurred while seeding data");
            throw new InvalidOperationException("An error occurred while seeding data.", ex);
        }
    }
}