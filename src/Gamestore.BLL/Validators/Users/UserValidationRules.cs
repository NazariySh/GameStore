using System.Text.RegularExpressions;
using Gamestore.DAL.Configurations.Users;

namespace Gamestore.BLL.Validators.Users;

public static partial class UserValidationRules
{
    public const int MinUserNameLength = 3;
    public const int MaxUserNameLength = UserConfiguration.MaxUserNameLength;
    public const int MinPasswordLength = UserConfiguration.MinPasswordLength;
    public const int MaxPasswordLength = 50;

    public const string UserNamePatternMessage = "Name can only contain latin letters, numbers and following characters: (-._@+)";

    [GeneratedRegex(@"^[A-Za-z\d-._@+]+$", RegexOptions.Compiled)]
    public static partial Regex UserNamePattern();
}