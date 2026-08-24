using System.Text;
using Gamestore.BLL.Extensions;
using Gamestore.BLL.Mapping.Games;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Settings;
using Gamestore.WebApi.Mapping.Games;
using Gamestore.WebApi.Middlewares;
using Gamestore.WebApi.Services.Auth;
using Gamestore.WebApi.Settings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Gamestore.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection<CorsSettings>(CorsSettings.SectionName);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(corsSettings.AllowedOrigins)
                    .WithHeaders(corsSettings.AllowedHeaders)
                    .WithMethods(corsSettings.AllowedMethods)
                    .WithExposedHeaders(corsSettings.ExposedHeaders);
            });
        });

        return services;
    }

    public static IServiceCollection AddOutputCachingPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheSettings = configuration.GetSection<CacheSettings>(CacheSettings.SectionName);

        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(policy =>
            {
                policy
                    .Expire(TimeSpan.FromMinutes(cacheSettings.OutputCacheExpirationInMinutes))
                    .Cache();
            });
        });

        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));

        return services;
    }

    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(GameMappingConfig).Assembly);
        config.Scan(typeof(GameModelMappingConfig).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    public static IServiceCollection AddCustomMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<RequestLoggingMiddleware>();
        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddTransient<TotalGamesHeaderMiddleware>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        var jwtSettings = configuration.GetSection<JwtSettings>(JwtSettings.SectionName);
        ArgumentException.ThrowIfNullOrEmpty(jwtSettings.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var authToken = context.Request.Headers.Authorization.FirstOrDefault();

                    if (!string.IsNullOrEmpty(authToken))
                    {
                        context.Token = authToken;
                    }

                    return Task.CompletedTask;
                },
            };
        });

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gamestore API", Version = "v1" });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        return services;
    }
}