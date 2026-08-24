namespace Gamestore.DAL.Data.Initializers.Interfaces;

public interface IDataUpdater
{
    int Order { get; }

    Task UpdateAsync(CancellationToken cancellationToken = default);
}