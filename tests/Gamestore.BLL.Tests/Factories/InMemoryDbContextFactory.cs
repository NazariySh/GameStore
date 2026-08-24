using Gamestore.DAL.Data;
using Gamestore.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.BLL.Tests.Factories;

public static class InMemoryDbContextFactory
{
    public static GamestoreDbContext Create(string databaseName)
    {
        Guard.AgainstNullOrWhiteSpace(databaseName);

        var options = new DbContextOptionsBuilder<GamestoreDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new GamestoreDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}