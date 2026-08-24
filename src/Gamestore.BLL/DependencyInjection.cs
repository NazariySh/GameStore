using System.Net;
using Azure.Identity;
using Azure.Storage.Blobs;
using FluentValidation;
using Gamestore.BLL.Data.Implementations;
using Gamestore.BLL.Data.Implementations.Initializers;
using Gamestore.BLL.Data.Interfaces;
using Gamestore.BLL.Extensions;
using Gamestore.BLL.Factories.Implementations;
using Gamestore.BLL.Factories.Interfaces;
using Gamestore.BLL.Integrations.Implementations;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Auth;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Orders;
using Gamestore.BLL.Interfaces.Payments;
using Gamestore.BLL.Interfaces.Payments.Processors;
using Gamestore.BLL.Interfaces.Shippers;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Auth;
using Gamestore.BLL.Services.Games;
using Gamestore.BLL.Services.Logging;
using Gamestore.BLL.Services.Orders;
using Gamestore.BLL.Services.Payments;
using Gamestore.BLL.Services.Payments.Processors;
using Gamestore.BLL.Services.Shippers;
using Gamestore.BLL.Services.Users;
using Gamestore.BLL.Utilities;
using Gamestore.BLL.Validators.Games;
using Gamestore.DAL.Data.Initializers.Interfaces;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Gamestore.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCaching();

        services.AddValidation();

        services.AddServices();

        services.ConfigureAzureClients(configuration);

        services.AddExternalAuthIntegration(configuration);
        services.AddPaymentIntegration(configuration);

        services.AddDataCoordination();
        services.AddInitializers();

        return services;
    }

    private static void AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddScoped<ICacheService, CacheService>();
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GameCreateDtoValidator>();

        services.AddScoped<IValidationService, ValidationService>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IOptionsProvider, OptionsProvider>();
        services.AddSingleton<INameNormalizer, NameNormalizer>();

        services.AddScoped<IEntityChangeLogService, EntityChangeLogService>();
        services.AddScoped<IServiceContext, ServiceContext>();

        services.AddScoped<IShipperService, ShipperService>();

        services.AddScoped<IGameKeyGenerator, GameKeyGenerator>();
        services.AddScoped<IGameImageService, GameImageService>();
        services.AddScoped<IGameService, GameService>();

        services.AddScoped<IPlatformService, PlatformService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IPublisherService, PublisherService>();

        services.AddScoped<ICommentService, CommentService>();

        services.AddScoped<IUserBanService, UserBanService>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();

        services.AddScoped<IPaymentService, PaymentService>();

        services.AddSingleton<IPaymentInvoiceGenerator, PdfPaymentInvoiceGenerator>();

        services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();
        services.AddScoped<IPaymentProcessor, BankPaymentProcessor>();
        services.AddScoped<IPaymentProcessor, BoxPaymentProcessor>();
        services.AddScoped<IPaymentProcessor, VisaPaymentProcessor>();

        services.AddSingleton<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
    }

    private static void AddDataCoordination(this IServiceCollection services)
    {
        services.AddScoped<IRepository<Game>, ProductRepositoryFacade>();
        services.AddScoped<IRepository<Genre>, CategoryRepositoryFacade>();
        services.AddScoped<IRepository<Publisher>, SupplierRepositoryFacade>();
        services.AddScoped<IRepository<Order>, RepositoryFacade<Order>>();
        services.AddScoped<IRepository<OrderGame>, RepositoryFacade<OrderGame>>();

        services.AddScoped<IGameRepository, ProductRepositoryFacade>();
        services.AddScoped<IProductRepositoryFacade, ProductRepositoryFacade>();
    }

    private static void AddInitializers(this IServiceCollection services)
    {
        services.AddScoped<IMongoDataUpdater, ProductKeyDataUpdater>();
    }

    private static void ConfigureAzureClients(this IServiceCollection services, IConfiguration configuration)
    {
        var blobSettings = configuration.GetSection<BlobStorageSettings>(BlobStorageSettings.SectionName);

        services.AddAzureClients(builder =>
        {
            builder.AddClient<BlobContainerClient, BlobClientOptions>(options =>
                    new BlobContainerClient(blobSettings.ConnectionString, blobSettings.GameContainerName, options))
                .WithName(blobSettings.GameContainerName);

            builder.UseCredential(new DefaultAzureCredential());
        });

        services.Configure<BlobStorageSettings>(configuration.GetSection(BlobStorageSettings.SectionName));
    }

    private static void AddExternalAuthIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        var authApiSettings = configuration.GetSection<AuthApiSettings>(AuthApiSettings.SectionName);

        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(authApiSettings.BaseUrl);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.Configure<AuthApiSettings>(configuration.GetSection(AuthApiSettings.SectionName));
    }

    private static void AddPaymentIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        var paymentSettings = configuration.GetSection<PaymentSettings>(PaymentSettings.SectionName);

        services.AddHttpClient<IPaymentApiClient, PaymentApiClient>(client =>
            {
                client.BaseAddress = new Uri(paymentSettings.BaseUrl);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy(paymentSettings.RetrySettings));

        services.Configure<PaymentSettings>(configuration.GetSection(PaymentSettings.SectionName));
        services.Configure<BankPaymentSettings>(configuration.GetSection(BankPaymentSettings.SectionName));
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(RetrySettings retrySettings)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode is
                HttpStatusCode.PaymentRequired or
                HttpStatusCode.MethodNotAllowed or
                HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retrySettings.MaxRetries,
                retryAttempt => TimeSpan.FromMilliseconds(retrySettings.DelayInMilliseconds * retryAttempt));
    }
}