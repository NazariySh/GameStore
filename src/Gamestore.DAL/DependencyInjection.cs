using Gamestore.DAL.Configurations.Users;
using Gamestore.DAL.Data;
using Gamestore.DAL.Data.Initializers.Implementations;
using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;
using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Data.Initializers.Implementations.DataUpdaters;
using Gamestore.DAL.Data.Initializers.Interfaces;
using Gamestore.DAL.Repositories.Implementations;
using Gamestore.DAL.Repositories.Implementations.MongoRepositories;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories.Games;
using Gamestore.DAL.Repositories.Implementations.SqlRepositories.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Logging;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Entities.Payments;
using Gamestore.Domain.Entities.Shippers;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Gamestore.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSqlServerDatabase(configuration);
        services.AddMongoDatabase(configuration);

        services.AddRepositories();

        services.AddInitializers();

        return services;
    }

    private static void AddSqlServerDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GamestoreDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("GamestoreDatabase");
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            options.UseSqlServer(connectionString);
        });

        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = UserConfiguration.MinPasswordLength;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<GamestoreDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NorthwindDatabase");
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        var mongoUrl = new MongoUrl(connectionString);

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.RegisterSerializer(new CustomNullableStringSerializer());
        BsonSerializer.RegisterSerializer(new CustomNullableDateTimeSerializer());

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUrl));

        services.AddScoped(sp =>
        {
            var mongoClient = sp.GetRequiredService<IMongoClient>();
            return new NorthwindMongoDbContext(mongoClient, mongoUrl.DatabaseName);
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepositoryProvider, RepositoryProvider>();

        services.AddScoped<ISqlRepository<Comment>, Repository<Comment>>();
        services.AddScoped<ISqlRepository<Platform>, Repository<Platform>>();
        services.AddScoped<ISqlRepository<Game>, GameRepository>();
        services.AddScoped<ISqlRepository<Genre>, Repository<Genre>>();
        services.AddScoped<ISqlRepository<Publisher>, Repository<Publisher>>();
        services.AddScoped<ISqlRepository<Order>, Repository<Order>>();
        services.AddScoped<ISqlRepository<OrderGame>, Repository<OrderGame>>();
        services.AddScoped<ISqlRepository<PaymentMethod>, Repository<PaymentMethod>>();
        services.AddScoped<ISqlRepository<UserBan>, Repository<UserBan>>();
        services.AddScoped<ISqlRepository<User>, Repository<User>>();
        services.AddScoped<ISqlRepository<Role>, Repository<Role>>();

        services.AddScoped<IMongoRepository<Publisher>, SupplierMongoRepository>();
        services.AddScoped<IMongoRepository<Genre>, CategoryMongoRepository>();
        services.AddScoped<IMongoRepository<Game>, ProductMongoRepository>();
        services.AddScoped<IMongoRepository<Order>, OrderMongoRepository>();
        services.AddScoped<IMongoRepository<OrderGame>, OrderDetailsMongoRepository>();
        services.AddScoped<IMongoRepository<Shipper>, ShipperMongoRepository>();
        services.AddScoped<IMongoRepository<EntityChangeLog>, EntityChangeLogMongoRepository>();

        services.AddScoped<IRepository<Shipper>, ShipperMongoRepository>();

        services.AddScoped<IRepository<Platform>, Repository<Platform>>();
        services.AddScoped<IRepository<PaymentMethod>, Repository<PaymentMethod>>();
        services.AddScoped<IRepository<UserBan>, Repository<UserBan>>();
        services.AddScoped<IRepository<Comment>, Repository<Comment>>();
        services.AddScoped<IRepository<User>, Repository<User>>();
        services.AddScoped<IRepository<Role>, Repository<Role>>();

        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddScoped<ProductMongoRepository>();
        services.AddScoped<GameRepository>();
    }

    private static void AddInitializers(this IServiceCollection services)
    {
        services.AddScoped<IDbInitializer, SqlDbInitializer>();
        services.AddScoped<IDbInitializer, MongoDbInitializer>();
        services.AddDataSeeders();
        services.AddDataUpdaters();
    }

    private static void AddDataSeeders(this IServiceCollection services)
    {
        services.AddSingleton<ISeedDataExtractor, JsonSeedDataExtractor>();

        services.AddScoped<ISqlDataSeeder, GenreDataSeeder>();
        services.AddScoped<ISqlDataSeeder, PlatformDataSeeder>();
        services.AddScoped<ISqlDataSeeder, PublisherDataSeeder>();
        services.AddScoped<ISqlDataSeeder, GameDataSeeder>();
        services.AddScoped<ISqlDataSeeder, OrderDataSeeder>();
        services.AddScoped<ISqlDataSeeder, PaymentMethodDataSeeder>();
        services.AddScoped<ISqlDataSeeder, CommentDataSeeder>();
        services.AddScoped<ISqlDataSeeder, RoleDataSeeder>();
        services.AddScoped<ISqlDataSeeder, UserDataSeeder>();
    }

    private static void AddDataUpdaters(this IServiceCollection services)
    {
        services.AddScoped<IMongoDataUpdater, ProductDataUpdater>();
        services.AddScoped<IMongoDataUpdater, OrderDataUpdater>();
    }
}