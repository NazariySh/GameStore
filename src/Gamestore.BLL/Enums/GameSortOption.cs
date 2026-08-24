namespace Gamestore.BLL.Enums;

public class GameSortOption : DisplayOption<GameSortOption>
{
    public static readonly GameSortOption MostPopular = new(nameof(MostPopular), 1, "Most popular");
    public static readonly GameSortOption MostCommented = new(nameof(MostCommented), 2, "Most commented");
    public static readonly GameSortOption PriceAsc = new(nameof(PriceAsc), 3, "Price ASC");
    public static readonly GameSortOption PriceDesc = new(nameof(PriceDesc), 4, "Price DESC");
    public static readonly GameSortOption New = new(nameof(New), 5, "New");

    private GameSortOption(string name, int value, string displayName)
        : base(name, value, displayName)
    {
    }

    public static GameSortOption? FromDisplayNameOrDefault(string? displayName)
    {
        return !string.IsNullOrEmpty(displayName)
            ? List.FirstOrDefault(x => x.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
            : null;
    }
}