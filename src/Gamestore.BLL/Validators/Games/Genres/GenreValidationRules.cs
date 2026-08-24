using System.Text.RegularExpressions;
using Gamestore.DAL.Configurations.Games;

namespace Gamestore.BLL.Validators.Games.Genres;

public static partial class GenreValidationRules
{
    public const int MinNameLength = 3;
    public const int MaxNameLength = GenreConfiguration.MaxNameLength;

    public const string NamePatternMessage = "Name can only contain letters, numbers, spaces and following characters: (-&'/)";

    [GeneratedRegex(@"^[\p{L}\d\s\-\&'/]+$", RegexOptions.Compiled)]
    public static partial Regex NamePattern();
}