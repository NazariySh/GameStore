namespace Gamestore.WebApi.Models.Games;

public abstract record CreateUpdateGameRequestModel
{
    public IReadOnlyList<string> Genres { get; init; }

    public IReadOnlyList<Guid> Platforms { get; init; }

    public string Publisher { get; init; }

    public string? Image { get; init; }
}