using System.Text.RegularExpressions;
using Gamestore.DAL.Configurations.Games;

namespace Gamestore.BLL.Validators.Games.Publishers;

public static partial class PublisherValidationRules
{
    public const int MinCompanyNameLength = 2;
    public const int MaxCompanyNameLength = PublisherConfiguration.MaxCompanyNameLength;
    public const int MaxHomePageLength = PublisherConfiguration.MaxHomePageLength;
    public const int MaxDescriptionLength = PublisherConfiguration.MaxDescriptionLength;

    public const string CompanyNamePatternMessage = "Company Name can only contain letters, numbers, spaces and following characters: (-'.,)";

    [GeneratedRegex(@"^[\p{L}\d\s\-'.,]+$", RegexOptions.Compiled)]
    public static partial Regex CompanyNamePattern();
}