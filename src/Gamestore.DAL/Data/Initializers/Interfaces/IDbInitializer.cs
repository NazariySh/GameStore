namespace Gamestore.DAL.Data.Initializers.Interfaces;

public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}