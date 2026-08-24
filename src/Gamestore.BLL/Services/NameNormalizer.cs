using System.Text.RegularExpressions;
using Gamestore.BLL.Interfaces;

namespace Gamestore.BLL.Services;

public partial class NameNormalizer : INameNormalizer
{
    private const char NameDelimiter = '-';

    public string Normalize(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        var matches = NameMatchPattern().Matches(normalizedName);
        return string.Join(NameDelimiter, matches.Select(m => m.Value));
    }

    [GeneratedRegex(@"[\p{L}\d]+", RegexOptions.Compiled)]
    private static partial Regex NameMatchPattern();
}