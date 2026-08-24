using Microsoft.EntityFrameworkCore.Storage;

namespace Gamestore.DAL.Repositories.Interfaces;

public interface IUnitOfWork
{
    IRepositoryProvider Repositories { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}