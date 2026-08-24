using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Implementations;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories.Games;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Entities.Payments;
using Gamestore.Domain.Entities.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Gamestore.BLL.Tests.Factories;

public static class UnitOfWorkFactory
{
    public static IUnitOfWork Create()
    {
        return Create(Mock.Of<IUserContext>());
    }

    public static IUnitOfWork Create(IUserContext userContext)
    {
        var context = CreateDbContext();
        var repositoryProvider = CreateRepositoryProvider(context, userContext);
        return new UnitOfWork(context, repositoryProvider);
    }

    private static RepositoryProvider CreateRepositoryProvider(GamestoreDbContext dbContext, IUserContext userContext)
    {
        var serviceProvider = BuildServiceProvider(dbContext, userContext);
        return new RepositoryProvider(serviceProvider);
    }

    private static ServiceProvider BuildServiceProvider(GamestoreDbContext dbContext, IUserContext userContext)
    {
        var services = new ServiceCollection();

        services.AddScoped(_ => dbContext);

        services.AddScoped(_ => userContext);

        services.AddScoped<IRepository<Comment>, Repository<Comment>>();
        services.AddScoped<IRepository<Platform>, Repository<Platform>>();
        services.AddScoped<IRepository<Game>, GameRepository>();
        services.AddScoped<IRepository<Genre>, Repository<Genre>>();
        services.AddScoped<IRepository<Publisher>, Repository<Publisher>>();
        services.AddScoped<IRepository<Order>, Repository<Order>>();
        services.AddScoped<IRepository<OrderGame>, Repository<OrderGame>>();
        services.AddScoped<IRepository<PaymentMethod>, Repository<PaymentMethod>>();
        services.AddScoped<IRepository<UserBan>, Repository<UserBan>>();
        services.AddScoped<IRepository<User>, Repository<User>>();
        services.AddScoped<IRepository<Role>, Repository<Role>>();

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services.BuildServiceProvider();
    }

    private static GamestoreDbContext CreateDbContext()
    {
        return InMemoryDbContextFactory.Create(GetRandomDatabaseName());
    }

    private static string GetRandomDatabaseName()
    {
        return $"GamestoreTestDb_{Guid.NewGuid()}";
    }
}