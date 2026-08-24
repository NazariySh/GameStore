using Gamestore.BLL.Interfaces.Games;

namespace Gamestore.WebApi.Middlewares;

public class TotalGamesHeaderMiddleware : IMiddleware
{
    private const string HeaderName = "x-total-numbers-of-games";

    private readonly IGameService _gameService;
    private readonly ILogger<TotalGamesHeaderMiddleware> _logger;

    public TotalGamesHeaderMiddleware(
        IGameService gameService,
        ILogger<TotalGamesHeaderMiddleware> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);

        var totalCount = await GetTotalGamesCountAsync();
        context.Response.Headers[HeaderName] = totalCount.ToString();

        _logger.LogInformation("Total games count header set to {TotalCount}", totalCount);
    }

    private async Task<int> GetTotalGamesCountAsync()
    {
        try
        {
            return await _gameService.GetTotalCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get total games count from cache or database");
            return 0;
        }
    }
}