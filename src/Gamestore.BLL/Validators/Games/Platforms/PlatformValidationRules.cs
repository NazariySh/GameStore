using System.Text.RegularExpressions;
using Gamestore.DAL.Configurations.Games;

namespace Gamestore.BLL.Validators.Games.Platforms;

public static partial class PlatformValidationRules
{
    public const int MinTypeLength = 2;
    public const int MaxTypeLength = PlatformConfiguration.MaxTypeLength;

    public const string TypePatternMessage = "Type can only contain letters, numbers, spaces and following characters: (-'.)";

    [GeneratedRegex(@"^[\p{L}\d\s\-'.]+$", RegexOptions.Compiled)]
    public static partial Regex TypePattern();
}