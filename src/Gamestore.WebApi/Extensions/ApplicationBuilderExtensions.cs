using Gamestore.DAL.Data.Initializers.Interfaces;

namespace Gamestore.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> InitializeAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var dbInitializers = scope.ServiceProvider.GetServices<IDbInitializer>();

        foreach (var dbInitializer in dbInitializers)
        {
            await dbInitializer.InitializeAsync();
        }

        return app;
    }
}