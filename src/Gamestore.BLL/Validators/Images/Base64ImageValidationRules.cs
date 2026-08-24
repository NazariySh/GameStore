using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Gamestore.BLL.Validators.Images;

public static partial class Base64ImageValidationRules
{
    public const string FormatPatternPatternMessage = "Image URL must be a valid base64-encoded data URL in format: data:mime/type;base64,...";

    public static readonly ImmutableHashSet<string> AllowedImageFormats =
    [
        "image/jpg",
        "image/jpeg",
        "image/png",
        "image/gif",
    ];

    [GeneratedRegex("data:([^;]+);base64,(.+)", RegexOptions.CultureInvariant)]
    public static partial Regex FormatPattern();
}