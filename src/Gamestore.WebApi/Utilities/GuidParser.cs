namespace Gamestore.WebApi.Utilities;

public static class GuidParser
{
    public static Guid? ParseGuid(this string? value)
    {
        return !string.IsNullOrEmpty(value)
            ? Guid.Parse(value)
            : null;
    }
}