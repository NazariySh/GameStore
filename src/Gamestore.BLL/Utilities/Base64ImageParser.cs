using Gamestore.BLL.Validators.Images;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Utilities;

public static class Base64ImageParser
{
    public static (byte[] Content, string ContentType) Parse(string dataUrl)
    {
        Guard.AgainstNullOrWhiteSpace(dataUrl);

        var base64Match = Base64ImageValidationRules.FormatPattern().Match(dataUrl);
        if (!base64Match.Success)
        {
            throw new ArgumentException(Base64ImageValidationRules.FormatPatternPatternMessage);
        }

        var contentType = base64Match.Groups[1].Value;
        var base64String = base64Match.Groups[2].Value;

        try
        {
            var imageBytes = Convert.FromBase64String(base64String);

            return (imageBytes, contentType);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Base64 string is not in a valid format.", ex);
        }
    }
}