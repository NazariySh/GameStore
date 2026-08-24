using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gamestore.DAL.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private const int SqlForeignKeyConstraintViolation = 547;
    private const int SqlUniqueConstraintViolationPrimaryKey = 2627;
    private const int SqlUniqueConstraintViolationIndex = 2601;

    private readonly GamestoreDbContext _dbContext;

    public UnitOfWork(
        GamestoreDbContext dbContext,
        IRepositoryProvider repositoryProvider)
    {
        _dbContext = dbContext;
        Repositories = repositoryProvider;
    }

    public IRepositoryProvider Repositories { get; }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            HandleDbUpdateConcurrencyException(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            HandleSqlException(sqlEx, ex);
        }
        catch (DbUpdateException ex)
        {
            HandleDbUpdateException(ex);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    private static void HandleDbUpdateConcurrencyException(DbUpdateConcurrencyException ex)
    {
        throw new InvalidOperationException(
            "A concurrency conflict occurred. The data may have been modified or deleted by another process.",
            ex);
    }

    private static void HandleSqlException(SqlException sqlEx, Exception original)
    {
        throw sqlEx.Number switch
        {
            SqlForeignKeyConstraintViolation when IsDeleteRestrictViolation(sqlEx) =>
                new InvalidOperationException(
                    "Deletion failed: This item is still referenced by other data and cannot be removed.",
                    original),
            SqlForeignKeyConstraintViolation =>
                new InvalidOperationException(
                    "Operation failed due to a foreign key constraint. Ensure all related data exists or is not dependent.",
                    original),
            SqlUniqueConstraintViolationPrimaryKey or SqlUniqueConstraintViolationIndex =>
                new InvalidOperationException(
                    "Duplicate entry detected. A unique constraint was violated.",
                    original),
            _ => new InvalidOperationException(
                $"A database error occurred. SQL error code: {sqlEx.Number}.",
                original),
        };
    }

    private static bool IsDeleteRestrictViolation(SqlException sqlEx)
    {
        return sqlEx.Message.Contains("DELETE", StringComparison.OrdinalIgnoreCase) &&
               sqlEx.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase);
    }

    private static void HandleDbUpdateException(DbUpdateException ex)
    {
        throw new InvalidOperationException(
            "An unexpected database update error occurred. Please check your data and try again.",
            ex);
    }
}