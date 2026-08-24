using System.Text.RegularExpressions;
using Gamestore.DAL.Configurations.Users;

namespace Gamestore.BLL.Validators.Users.Roles;

public static partial class RoleValidationRules
{
    public const int MinNameLength = 2;
    public const int MaxNameLength = RoleConfiguration.MaxRoleNameLength;

    public const string NamePatternMessage = "Name can only contain latin letters and spaces";

    [GeneratedRegex(@"^[A-Za-z\s]+$", RegexOptions.Compiled)]
    public static partial Regex NamePattern();
}