namespace Gamestore.DAL.Data.Initializers.Interfaces;

public interface IDataSeeder
{
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}