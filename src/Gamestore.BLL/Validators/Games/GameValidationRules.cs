using System.Text.RegularExpressions;
using Gamestore.DAL.Configurations.Games;

namespace Gamestore.BLL.Validators.Games;

public static partial class GameValidationRules
{
    public const int MinNameLength = 3;
    public const int MaxNameLength = GameConfiguration.MaxNameLength;
    public const int MinKeyLength = 3;
    public const int MaxKeyLength = GameConfiguration.MaxKeyLength;
    public const int MaxDescriptionLength = GameConfiguration.MaxDescriptionLength;
    public const decimal MinPrice = 0m;
    public const decimal MaxPrice = 10_000m;
    public const int MinDiscount = 0;
    public const int MaxDiscount = 100;

    public const string NamePatternMessage = "Name can only contain letters, numbers, spaces and following characters: (-'.:)";
    public const string KeyPatternMessage = "Key can only contain lowercase letters, numbers, and hyphens.";

    [GeneratedRegex(@"^[\p{L}\d\s\-'.:]+$", RegexOptions.Compiled)]
    public static partial Regex NamePattern();

    [GeneratedRegex(@"^[\p{Ll}\d-]+$", RegexOptions.Compiled)]
    public static partial Regex KeyPattern();
}