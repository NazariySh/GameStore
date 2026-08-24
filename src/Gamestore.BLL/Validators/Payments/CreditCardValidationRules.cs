using System.Text.RegularExpressions;

namespace Gamestore.BLL.Validators.Payments;

public static partial class CreditCardValidationRules
{
    public const int MinHolderLength = 5;
    public const int MaxHolderLength = 100;
    public const int MinMonth = 1;
    public const int MaxMonth = 12;
    public const int MaxYearExpireOffset = 25;
    public const int MinCvv = 001;
    public const int MaxCvv = 999;

    public const string HolderPatternMessage = "Card holder name can only contain latin letters, spaces, and apostrophes.";

    [GeneratedRegex(@"^[A-Za-z\s']+$", RegexOptions.Compiled)]
    public static partial Regex HolderPattern();
}